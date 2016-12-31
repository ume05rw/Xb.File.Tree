using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpCifs.Smb;
using Xb.App;
using Xb.File.Tree;
using Xb.Net;

namespace TestXb
{
    [TestClass()]
    public class ZipStreamTests
    {
        [TestMethod()]
        public async Task Streamtest()
        {
            return;
            var path = "zip file path";
            var bytes = System.IO.File.ReadAllBytes(path);
            var testStream = new TestStream();
            testStream.Stream = new MemoryStream(bytes);

            try
            {
                var zip = await Xb.File.ZipTree.GetTreeAsync(testStream);
                //var node = zip.Nodes.FirstOrDefault(n => n.Type == NodeBase.NodeType.File);
                var node = zip.Nodes.Where(n => n.Type == NodeBase.NodeType.File)
                                    .Skip(100)
                                    .Take(1)
                                    .ToArray()[0];
                var resultBytes = node.GetBytes();

                zip.Dispose();
            }
            catch (Exception ex)
            {
                testStream.Logger.Write(ex);
                throw ex;
            }

            SmbFile a = new SmbFile("aa");
            var st = a.GetInputStream();
        }

        [TestMethod()]
        public async Task Streamtest2()
        {
            return;
            var newSmbFile = new SmbFile("smb://uri_string");
            var stream = newSmbFile.GetInputStream();

            var toStream = (Stream)stream;

            var zip = await Xb.File.ZipTree.GetTreeAsync(stream);
            var node = zip.Nodes.Where(n => n.Type == NodeBase.NodeType.File)
                                .Skip(100)
                                .Take(1)
                                .ToArray()[0];
            var resultBytes = node.GetBytes();

            zip.Dispose();

        }


    }

    public class TestStream : Stream
    {
        public MemoryStream Stream;
        public Xb.App.Logger Logger;

        public TestStream()
        {
            this.Stream = new MemoryStream();
            this.Logger = new Logger($"StreamAccessLog_{DateTime.Now:MMdd_HHmm}.log");
        }

        public override bool CanRead
        {
            get
            {
                this.Logger.Write("Read CanRead");
                return this.Stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                this.Logger.Write("Read CanSeek");
                return this.Stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                this.Logger.Write("Read CanWrite");
                return this.Stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                this.Logger.Write("Read Length");
                return this.Stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                this.Logger.Write("Read Position");
                return this.Stream.Position;
            }

            set
            {
                this.Logger.Write("Write Position");
                this.Stream.Position = value;
            }
        }


        public override void Flush()
        {
            this.Logger.Write("Exec Flush");
            this.Stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.Logger.Write("Exec Read");
            return this.Stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.Logger.Write("Exec Seek");
            return this.Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.Logger.Write("Exec SetLength");
            this.Stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.Logger.Write("Exec Write");
            this.Stream.Write(buffer, offset, count);
        }
    }
}
