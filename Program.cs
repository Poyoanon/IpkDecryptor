using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine(@"Drag and drop the ExtRes folder (usually found in C:\Program Files (x86)\Steam\steamapps\common\Lost Ark\EFGame\ReleasePC\Packages\ExtRes) onto this executable to decrypt files.");
            return;
        }

        string folderPath = args[0];
        string extension = ".ipk";
        byte[] key = new byte[] { 0xe2, 0xc8, 0x4e, 0x1b, 0x78, 0xc7 };
        string outputFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");

        DecryptFilesInFolder(folderPath, extension, key, outputFolderPath);
    }

    static void DecryptFilesInFolder(string folderPath, string extension, byte[] key, string outputFolderPath)
    {
        DecryptFilesInSubFolder(folderPath, extension, key, outputFolderPath, "", "EXT://EXTRES\\");
    }

    static void DecryptFilesInSubFolder(string folderPath, string extension, byte[] key, string outputFolderPath, string relativePath, string extPath)
    {
        string subOutputFolderPath = Path.Combine(outputFolderPath, relativePath);
        Directory.CreateDirectory(subOutputFolderPath);

        foreach (string file in Directory.GetFiles(folderPath, "*" + extension, SearchOption.TopDirectoryOnly))
        {
            string encryptedFilePath = file;
            byte[] encryptedData = File.ReadAllBytes(encryptedFilePath);
            string decryptedFileName = Path.GetFileNameWithoutExtension(encryptedFilePath) + "_decrypted.png";
            string decryptedFilePath = Path.Combine(subOutputFolderPath, decryptedFileName);

            byte[] md5 = MD5Sum(extPath + Path.GetFileName(encryptedFilePath).ToUpper());
            byte[] expandedMd5 = ExpandBuffer(md5, encryptedData.Length);
            byte[] expandedKey = ExpandBuffer(key, encryptedData.Length);
            byte[] xorKey = XorBuffers(expandedMd5, expandedKey);
            byte[] decryptedData = XorBuffers(encryptedData, xorKey);

            File.WriteAllBytes(decryptedFilePath, decryptedData);
            Console.WriteLine($"Decrypted file written to: {decryptedFilePath}");
        }

        foreach (string subFolder in Directory.GetDirectories(folderPath))
        {
            string subRelativePath = Path.Combine(relativePath, subFolder.Substring(folderPath.Length).TrimStart('\\'));
            DecryptFilesInSubFolder(subFolder, extension, key, outputFolderPath, subRelativePath, extPath + subFolder.Substring(folderPath.Length).TrimStart('\\').ToUpper() + "\\");
        }
    }

    static byte[] MD5Sum(string data)
    {
        using (MD5 md5 = MD5.Create())
        {
            return md5.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }

    static byte[] XorBuffers(byte[] buffer, byte[] key)
    {
        byte[] result = new byte[buffer.Length];
        for (int i = 0; i < buffer.Length; i++)
        {
            result[i] = (byte)(buffer[i] ^ key[i % key.Length]);
        }
        return result;
    }

    static byte[] ExpandBuffer(byte[] buffer, int length)
    {
        byte[] expanded = new byte[length];
        for (int i = 0; i < length; i++)
        {
            expanded[i] = buffer[i % buffer.Length];
        }
        return expanded;
    }
}