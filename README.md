# OatmealDome.Cpio

This library allows you to access individual files within a CPIO archive file. Only the "old binary" format is supported. It is not possible to export files at this time.

## Usage

```csharp
using FileStream fileStream = File.OpenRead("/path/to/archive.cpio");
CpioArchive archive = new CpioArchive(fileStream);

// Fetch a file
byte[] file = archive["file.dat"];

// Get all files in the archive
foreach (KeyValuePair<string, byte[]> pair in archive)
{
    // Do something
}
```

# Credits

Thanks to Kasadee's [rpaextract](https://github.com/Kaskadee/rpaextract) and [Shizmob's rpatool](https://github.com/Shizmob/rpatool) for details about the Ren'Py archive format.
