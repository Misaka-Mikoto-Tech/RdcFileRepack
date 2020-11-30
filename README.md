# RdcFileRepack
> 用于导出RenderDoc文件的贴图以及重新打包

### 用法
  1. RdcFileRepack <rdc filepath> dump
  2. 使用PS修改`Export_<filapath>`目录下的两张缩略图以及`Textures`目录下的`<id>_SwapChain-BackBuffer-Texture-<size>.tga` 以及`<id>_Font Texture.bmp`, **请务必注意 `SwapChain-BackBuffer` 贴图有a通道，并且a通道也有数据，需要一并修改**
  3. RdcFileRepack <rdc filepath> repack
  4. 使用RenderDoc打开`<filepath>_repack.rdc`,执行 `Tools/Recompress Capture`

### 说明
  *  当前仅支持 D3D11
  * 当前仅支持导出非压缩格式贴图(可自行修改`D3DTextureConvert.SaveTextureToFile`)
