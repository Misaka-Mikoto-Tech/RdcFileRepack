using FreeImageAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rdc
{
    /// <summary>
    /// 表示一个rdc文件
    /// </summary>
    public unsafe class RdcFile
    {
        public string path { get; private set; }
        public ChunkManager chunkManager { get; private set; }

        private FileHeader _fileHeader;
        private BinaryThumbnail _binaryThumbnail;
        private CaptureMetaData _meta;
        private List<Section> _sections;

        public Section frameCaptureSection
        {
            get
            {
                int idx = SectionIndex(SectionType.FrameCapture);
                if (idx >= 0)
                    return _sections[idx];

                return null;
            }
        }

        public Section thumbnailSection
        {
            get
            {
                int idx = SectionIndex(SectionType.ExtendedThumbnail);
                if (idx >= 0)
                    return _sections[idx];

                return null;
            }
        }

        public void SetDeviceName(string name)
        {
            chunkManager.SetDeviceName(name);
        }

        /// <summary>
        /// 移除指定范围的chunk
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void RemoveChunkByEventId(int from, int to)
        {
            frameCaptureSection.RemoveChunkByEventId(from, to);
        }

        /// <summary>
        /// 自动修改UID使用的Vertexbuffer数据
        /// </summary>
        public void AutoModifyUidBuffer()
        {
            int eventId = 0;
            var metas = frameCaptureSection.chunkMetas;
            for(int i = metas.Count - 1; i >= 0; i--)
            {
                if(metas[i].chunkType == D3D11Chunk.IASetVertexBuffers)
                {
                    eventId = metas[i].eventId;
                    break;
                }
            }

            if (eventId > 0)
                ModifyUidBufferByEventid(eventId, 0);
        }

        /// <summary>
        /// 修改UID drawcall使用的顶点缓冲区, 并使用指定索引的数字填充
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="fillWithNumIndex"></param>
        /// <param name="characterWidth">字符宽度</param>
        public void ModifyUidBufferByEventid(int eventId, int fillWithNumIndex, float characterWidth = 8f)
        {
            UIDBufferModifier.Modify(frameCaptureSection.chunkManager, eventId, fillWithNumIndex, characterWidth);
        }

        public void LoadFromRdc(string path)
        {
            Console.WriteLine($"正在打开文件 {path}");
            this.path = path;

            FileStream fs;
            BinaryReader br;
            try
            {
                fs = File.OpenRead(path);
                br = new BinaryReader(fs);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"打开文件 {path} 失败,msg:{ex.Message}");
                return;
            }

            try
            {
                _fileHeader = new FileHeader();
                _fileHeader.LoadFromStream(br);
                if (!_fileHeader.IsValid())
                    return;

                _binaryThumbnail = new BinaryThumbnail();
                _binaryThumbnail.LoadFromStream(br);

                _meta = new CaptureMetaData();
                _meta.LoadFromStream(br);

                br.BaseStream.Position += _fileHeader.headerLength - br.BaseStream.Position; // skip bytes

                // read sections
                _sections = new List<Section>();
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var section = new Section();
                    section.LoadFromStream(br);

                    _sections.Add(section);
                }
            }
            finally
            {
                fs.Close();
            }

            if(SectionIndex(SectionType.FrameCapture) == -1)
                throw new Exception("Capture file doesn't have a frame capture");
            else
            {
                chunkManager = frameCaptureSection.chunkManager;
            }

            Console.WriteLine($"打开文件 {path} 成功");
            return;
        }

        public bool LoadFromJson(string dir)
        {
            throw new NotImplementedException();
        }

        public bool SaveToRdc(string path)
        {
            FileStream fs;
            BinaryWriter bw;
            try
            {
                File.Delete(path);
                fs = File.OpenWrite(path);
                bw = new BinaryWriter(fs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打开文件 {path} 失败,msg:{ex.Message}");
                return false;
            }

            try
            {
                _fileHeader.SaveToStream(bw);
                _binaryThumbnail.SaveToStream(bw);
                _meta.SaveToStream(bw);

                {// write padding
                    int paddingSize = (int)(_fileHeader.headerLength - bw.BaseStream.Position);
                    for (int i = 0; i < paddingSize; i++)
                        bw.Write((byte)0);
                }

                foreach (var section in _sections)
                    section.SaveToStream(bw);
            }
            finally
            {
                fs.Close();
            }

            return true;
        }

        public bool SaveToJson(string dir)
        {
            return false;
        }

        public int SectionIndex(SectionType type)
        {
            if (type == SectionType.Unknown)
                return -1;

            for(int i = 0; i < _sections.Count; i++)
            {
                if (_sections[i].header.sectionType == type)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// 导出缩略图
        /// </summary>
        public void ExportThumbnail()
        {
            // rdc 内有两个缩略图，一个是紧邻FileHeader的BinaryThumbnail，一个是独立的Section
            string exportDir = $"{Path.GetDirectoryName(path)}/Export_{Path.GetFileNameWithoutExtension(path)}";
            Directory.CreateDirectory(exportDir);

            File.WriteAllBytes($"{exportDir}/BinaryThumbnail.jpg", _binaryThumbnail.data);

            var thumbnailSection = this.thumbnailSection;
            if (thumbnailSection != null)
            {
                File.WriteAllBytes($"{exportDir}/ExtThumbnail.png", thumbnailSection.thumbPixels);
            }
        }

        /// <summary>
        /// 载入缩略图
        /// </summary>
        public void LoadThumbnail()
        {
            string exportDir = $"{Path.GetDirectoryName(path)}/Export_{Path.GetFileNameWithoutExtension(path)}";
            string binaryThumbnailPath = $"{exportDir}/BinaryThumbnail.jpg";
            if (File.Exists(binaryThumbnailPath))
            {
                _binaryThumbnail.data = File.ReadAllBytes(binaryThumbnailPath);
            }
            else
            {
                Console.WriteLine($"没有找到文件 {binaryThumbnailPath}");
            }

            var thumbnailSection = this.thumbnailSection;
            if (thumbnailSection != null)
            {
                string extThumbnailPath = $"{exportDir}/ExtThumbnail.png";
                if (!File.Exists(extThumbnailPath))
                    return;

                var dib = FreeImage.Load(FREE_IMAGE_FORMAT.FIF_PNG, extThumbnailPath, FREE_IMAGE_LOAD_FLAGS.DEFAULT);
                if(dib != null && !dib.IsNull)
                {
                    uint w = FreeImage.GetWidth(dib);
                    uint h = FreeImage.GetHeight(dib);
                    var thumbHeader = thumbnailSection.thumbHeader;
                    if(w == thumbHeader.width && h == thumbHeader.height)
                    {
                        byte[] buff = File.ReadAllBytes(extThumbnailPath);
                        thumbnailSection.thumbPixels = buff;
                        thumbHeader.len = (uint)buff.Length;
                    }
                    FreeImage.Unload(dib);
                }
            }
        }

        /// <summary>
        /// 导出所有支持的贴图
        /// </summary>
        public void ExportTextures()
        {
            string exportDir = $"{Path.GetDirectoryName(path)}/Export_{Path.GetFileNameWithoutExtension(path)}";
            Directory.CreateDirectory(exportDir);
            exportDir += "/Textures";
            Directory.CreateDirectory(exportDir);

            foreach (var kv in chunkManager.resourceChunks)
            {
                IChunk chunk = kv.Value;
                if (chunk.name == null || chunk.resourceId == 0)
                    continue;

                string path = $"{exportDir}/{chunk.resourceId}_{chunk.name}";

                if (kv.Value is Chunk_CreateTexture2D)
                {
                    Chunk_CreateTexture2D texChunk = kv.Value as Chunk_CreateTexture2D;

                    D3DTextureConvert.SaveTextureToFile(texChunk, path);
                }
                else if(kv.Value is Chunk_CreateSwapBuffer)
                {
                    Chunk_CreateSwapBuffer swapChunk = kv.Value as Chunk_CreateSwapBuffer;

                    D3DTextureConvert.SaveTextureToFile(swapChunk, path);
                }
            }
        }

        /// <summary>
        /// 加载所有贴图数据并替换内存数据
        /// </summary>
        public void LoadTexturesFromFile()
        {
            string exportDir = $"{Path.GetDirectoryName(path)}/Export_{Path.GetFileNameWithoutExtension(path)}/Textures";
            if (!Directory.Exists(exportDir))
                return;

            string [] files = Directory.GetFiles(exportDir, "*.*", SearchOption.TopDirectoryOnly);

            Regex regex = new Regex(@"(\d+)_.*");
            foreach(string path in files)
            {
                Match match = regex.Match(Path.GetFileName(path));
                if (!match.Success)
                    continue;

                int resourceId = int.Parse(match.Groups[1].Value);
                IChunk chunk = chunkManager.GetResourceChunk((ulong)resourceId);

                if(chunk is Chunk_CreateTexture2D)
                {
                    Chunk_CreateTexture2D texChunk = chunk as Chunk_CreateTexture2D;

                    D3DTextureConvert.LoadTextureDataFromFile(texChunk, path);
                }
                else if (chunk is Chunk_CreateSwapBuffer)
                {
                    Chunk_CreateSwapBuffer swapChunk = chunk as Chunk_CreateSwapBuffer;

                    D3DTextureConvert.LoadTextureDataFromFile(swapChunk, path);
                }
            }
        }

        /// <summary>
        /// 导出所有的buffer，注意只能导出此buffer初始数据，数据在多个drawcall中可能被 Unmap 指令修改
        /// </summary>
        public void ExportBuffers()
        {

        }

        public void DumpChunks()
        {
            string infos = chunkManager.DumpChunkInfos();
            string chunkFilePath = $"{Path.GetDirectoryName(path)}/Export_{Path.GetFileNameWithoutExtension(path)}/ChunkInfos.txt";
            File.WriteAllText(chunkFilePath, infos);
        }

        public void DumpAll()
        {
            ExportThumbnail();
            ExportTextures();
            ExportBuffers();
            DumpChunks();
        }
    }

}
