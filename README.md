# RdcFileRepack
> 用于导出RenderDoc文件的贴图以及重新打包

### 用法
  * RdcFileRepack [filepath] [mode]
  * mode 可选 `dump` 或 `repack`

### 说明
  *  当前仅支持 D3D11
  * 当前仅支持导出非压缩格式贴图(可自行修改`D3DTextureConvert.SaveTextureToFile`)
