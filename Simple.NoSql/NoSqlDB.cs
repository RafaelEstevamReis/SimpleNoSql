using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Simple.NoSql
{
    public class NoSqlDB : IDisposable
    {
        readonly SHA256 hasher;

        public DirectoryInfo Directory { get; }
        public DatabaseVersion DatabaseVersion { get; }

        public NoSqlDB(string directory)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            if (!di.Exists) di.Create();
            Directory = di;
            DatabaseVersion = DatabaseVersion.V1_0;

            hasher = SHA256.Create();
        }

        public void Insert<T>(string key, T obj)
        {
            string path = getKeyFilePath(key);
            FileInfo fi = new FileInfo(path);
            if (!fi.Directory.Exists) fi.Directory.Create();

            var container = new DataContainer() { Key = key, };
            using (var ms = new MemoryStream())
            {
                writeBson(obj, ms);
                container.Data = ms.ToArray();
            }

            using (var fs = new FileStream(fi.FullName, FileMode.Create, FileAccess.Write))
            {
                writeBson(container, fs);
            }
        }
        public T Get<T>(string key)
        {
            string path = getKeyFilePath(key);
            FileInfo fi = new FileInfo(path);
            if (!fi.Exists) throw new FileNotFoundException();

            using var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var container = readBson<DataContainer>(fs);

            using var ms = new MemoryStream(container.Data);
            ms.Position = 0;
            return readBson<T>(ms);
        }
        public IEnumerable<string> GetAllKeys()
        {
            foreach (var f in getAllDataFiles())
            {
                using var fs = new FileStream(f.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                var container = readBson<DataContainer>(fs);
                yield return container.Key;
            }
        }
        public IEnumerable<FileInfo> getAllDataFiles()
        {
            foreach (var d1 in Directory.GetDirectories())
            {
                foreach (var d2 in d1.GetDirectories())
                {
                    foreach (var file in d2.GetFiles("*.data"))
                    {
                        yield return file;
                    }
                }
            }
        }

        private string getKeyFileName(string key)
        {
            var bKey = Encoding.UTF8.GetBytes(key);
            var hex = fastToHex(hasher.ComputeHash(bKey));
            return $"{hex}.data";
        }
        private string getKeyFilePath(string key)
        {
            var hexKey = getKeyFileName(key);
            return Path.Combine(Directory.FullName, hexKey[0..2], hexKey[0..4], hexKey);
        }

        private static void writeBson<T>(T obj, Stream destinationStream)
        {
            if (obj is null) return;

            using var writer = new BsonDataWriter(destinationStream);
            var serializer = new JsonSerializer();
            serializer.Serialize(writer, obj);
        }
        private static T readBson<T>(Stream sourceStream)
        {
            using var reader = new BsonDataReader(sourceStream);
            JsonSerializer serializer = new JsonSerializer();
            return serializer.Deserialize<T>(reader);
        }

        public void Dispose()
        {
            hasher.Dispose();
        }

        #region Static 
        static readonly Dictionary<byte, string> hexTable;
        static NoSqlDB()
        {
            hexTable = new Dictionary<byte, string>();
            createHexTable();
        }
        private static void createHexTable()
        {
            for (int i = 0; i <= 0xFF; i++)
            {
                hexTable.Add((byte)i, i.ToString("X2"));
            }
        }
        static string fastToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);

            foreach (byte b in bytes)
            {
                sb.Append(hexTable[b]);
            }

            return sb.ToString();
        }

        #endregion

    }
}
