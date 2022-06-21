using System.Collections;
using System.Text;
using OatmealDome.BinaryData;

namespace OatmealDome.Cpio;

public class CpioArchive : IEnumerable<KeyValuePair<string, byte[]>>
{
    private const ushort CpioBinaryMagic = 0x71c7; // 070707 in octal

    private Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>();
    
    public byte[] this[string key]
    {
        get => _files[key];
        set => throw new NotImplementedException();
    }

    public CpioArchive(Stream stream, ByteOrder byteOrder = ByteOrder.LittleEndian)
    {
        using BinaryDataReader reader = new BinaryDataReader(stream);
        reader.ByteOrder = byteOrder;

        ushort headerStart;
        using (reader.TemporarySeek())
        {
            headerStart = reader.ReadUInt16();
        }

        if (headerStart == CpioBinaryMagic)
        {
            ReadOldBinaryFormat(reader);
        }
        else
        {
            throw new CpioException("Unsupported CPIO archive format");
        }
    }

    public CpioArchive(byte[] rawData, ByteOrder byteOrder = ByteOrder.LittleEndian) : this(new MemoryStream(rawData),
        byteOrder)
    {
        //
    }
    
    // Format documentation can be found at:
    // https://manpages.ubuntu.com/manpages/bionic/en/man5/cpio.5.html

    private void ReadOldBinaryFormat(BinaryDataReader reader)
    {
        while (true)
        {
            if (reader.ReadUInt16() != CpioBinaryMagic)
            {
                throw new CpioException("Invalid header magic for CPIO archive entry");
            }

            int ReadInt32()
            {
                // Odd way of storing this, but OK...
                return reader.ReadUInt16() << 16 | reader.ReadUInt16();
            }

            void AlignPositionToEvenBoundary()
            {
                if (reader.Position % 2 != 0)
                {
                    reader.Seek(1);
                }
            }

            reader.Seek(2); // c_dev
            reader.Seek(2); // c_ino
            reader.Seek(2); // c_mode
            reader.Seek(2); // c_uid;
            reader.Seek(2); // c_gid;
            reader.Seek(2); // c_nlink;
            reader.Seek(2); // c_rdev;
            reader.Seek(4); // c_mtime
            reader.Seek(2); // c_namesize - we can just read to the NULL terminator

            int dataSize = ReadInt32(); // c_filesize
            string name = reader.ReadString(StringDataFormat.ZeroTerminated, Encoding.UTF8);

            // An entry with this file name indicates that this is the trailing entry for this archive.
            if (name == "TRAILER!!!")
            {
                break;
            }
            
            AlignPositionToEvenBoundary();

            byte[] data = reader.ReadBytes(dataSize);
            
            AlignPositionToEvenBoundary();
            
            _files.Add(name, data);
        }
    }

    public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator()
    {
        return _files.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
