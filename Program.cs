using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine(
                @"Drag and drop the ExtRes folder (usually found in C:\Program Files (x86)\Steam\steamapps\common\Lost Ark\EFGame\ReleasePC\Packages\ExtRes) or the Movies folder (found in C:\Program Files (x86)\Steam\steamapps\common\Lost Ark\EFGame\Movies) onto this executable to decrypt files."
            );
            return;
        }

        string folderPath = args[0];
        const string extension = ".ipk";
        byte[] key = [0xe2, 0xc8, 0x4e, 0x1b, 0x78, 0xc7];
        //byte[] key = [0x2a, 0x4e, 0x21, 0xe6, 0x10, 0x8a]; KR Version
        string outputFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");

        string outputExtension = "";
        string extPath = "";
        if (folderPath.EndsWith("ExtRes", StringComparison.OrdinalIgnoreCase))
        {
            extPath = @"EXT://EXTRES\";
            outputExtension = ".png";
        }
        else if (folderPath.EndsWith("Movies", StringComparison.OrdinalIgnoreCase))
        {
            extPath = @"EXT://MOVIES\";
            outputExtension = ".bk2";
        }

        DecryptFilesInSubFolder(
            folderPath,
            extension,
            key,
            outputFolderPath,
            "",
            extPath,
            outputExtension
        );
    }

    static void DecryptFilesInSubFolder(
        string folderPath,
        string extension,
        byte[] key,
        string outputFolderPath,
        string relativePath,
        string extPath,
        string outputExtension
    )
    {
        string subOutputFolderPath = Path.Combine(outputFolderPath, relativePath);
        Directory.CreateDirectory(subOutputFolderPath);

        foreach (
            string file in Directory.GetFiles(
                folderPath,
                "*" + extension,
                SearchOption.TopDirectoryOnly
            )
        )
        {
            string encryptedFilePath = file;
            byte[] encryptedData = File.ReadAllBytes(encryptedFilePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(encryptedFilePath);

            string[] parts = fileNameWithoutExtension.Split('.');
            if (
                parts.Length > 1
                    && parts[parts.Length - 1]
                        .EndsWith("English", StringComparison.OrdinalIgnoreCase)
                || parts[parts.Length - 1].EndsWith("French", StringComparison.OrdinalIgnoreCase)
                || parts[parts.Length - 1].EndsWith("German", StringComparison.OrdinalIgnoreCase)
                || parts[parts.Length - 1].EndsWith("Spanish", StringComparison.OrdinalIgnoreCase)
            )
            {
                fileNameWithoutExtension = string.Join(".", parts.Take(parts.Length - 1));
            }

            string deobfuscatedFileName = Deobfuscate.Deobfuscate.Decrypt(fileNameWithoutExtension);
            var decryptedFileName = $"{deobfuscatedFileName}{outputExtension}";
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
            string subRelativePath = Path.Combine(
                relativePath,
                subFolder.Substring(folderPath.Length).TrimStart('\\')
            );
            DecryptFilesInSubFolder(
                subFolder,
                extension,
                key,
                outputFolderPath,
                subRelativePath,
                @$"{extPath}{subFolder.Substring(folderPath.Length).TrimStart('\\').ToUpper()}\",
                outputExtension
            );
        }
    }

    static byte[] MD5Sum(string data)
    {
        using MD5 md5 = MD5.Create();
        return md5.ComputeHash(Encoding.UTF8.GetBytes(data));
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
