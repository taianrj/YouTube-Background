$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
public static class BrandArt {
 public static Bitmap Icon(int size) {
  var big = new Bitmap(size*4,size*4);
  using(var g=Graphics.FromImage(big)) {
   g.SmoothingMode=SmoothingMode.AntiAlias; g.ScaleTransform(size*4/64f,size*4/64f);
   using(var path=new GraphicsPath()) {
    path.AddArc(2,2,30,30,180,90);path.AddArc(32,2,30,30,270,90);path.AddArc(32,32,30,30,0,90);path.AddArc(2,32,30,30,90,90);path.CloseFigure();
    using(var brush=new LinearGradientBrush(new Point(2,2),new Point(62,62),Color.FromArgb(255,87,94),Color.FromArgb(217,35,66))) g.FillPath(brush,path);
   }
   using(var pen=new Pen(Color.White,6)) {pen.StartCap=LineCap.Round;pen.EndCap=LineCap.Round;pen.LineJoin=LineJoin.Round;
    g.DrawLines(pen,new PointF[]{new PointF(25,20),new PointF(13,32),new PointF(25,44)});
    g.DrawLines(pen,new PointF[]{new PointF(39,20),new PointF(51,32),new PointF(39,44)});
   }
  }
  var result=new Bitmap(size,size);
  using(var g=Graphics.FromImage(result)){g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawImage(big,0,0,size,size);}
  big.Dispose();return result;
 }
 public static void Wizard(string path) {
  using(var bmp=new Bitmap(328,628)) using(var g=Graphics.FromImage(bmp)) {
   g.Clear(Color.FromArgb(248,249,252));g.SmoothingMode=SmoothingMode.AntiAlias;
   using(var b=new SolidBrush(Color.FromArgb(255,230,232)))g.FillEllipse(b,-200,330,600,600);
   using(var b=new SolidBrush(Color.FromArgb(250,204,211)))g.FillEllipse(b,150,450,350,350);
   using(var logo=Icon(168))g.DrawImage(logo,80,90,168,168);
   bmp.Save(path,System.Drawing.Imaging.ImageFormat.Bmp);
  }
 }
}
'@
$sizes = @(16,20,24,32,40,48,64,128,256)
$images = @()
foreach ($size in $sizes) {
    $bitmap = [BrandArt]::Icon($size)
    $memory = New-Object IO.MemoryStream
    try { $bitmap.Save($memory, [Drawing.Imaging.ImageFormat]::Png); $images += ,$memory.ToArray() }
    finally { $memory.Dispose(); $bitmap.Dispose() }
}
$file = [IO.File]::Create((Join-Path $PSScriptRoot 'app.ico'))
$writer = New-Object IO.BinaryWriter($file)
try {
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$sizes.Count)
    $offset = 6 + 16 * $sizes.Count
    for ($i=0; $i -lt $sizes.Count; $i++) {
        $dimension = if ($sizes[$i] -eq 256) { 0 } else { $sizes[$i] }
        $writer.Write([byte]$dimension);$writer.Write([byte]$dimension);$writer.Write([byte]0);$writer.Write([byte]0)
        $writer.Write([uint16]1);$writer.Write([uint16]32);$writer.Write([uint32]$images[$i].Length);$writer.Write([uint32]$offset)
        $offset += $images[$i].Length
    }
    foreach ($bytes in $images) { $writer.Write([byte[]]$bytes) }
} finally { $writer.Dispose(); $file.Dispose() }
$preview = [BrandArt]::Icon(256)
try { $preview.Save((Join-Path $PSScriptRoot 'icon.png'), [Drawing.Imaging.ImageFormat]::Png) } finally { $preview.Dispose() }
$small = [BrandArt]::Icon(110)
$smallBackground = New-Object Drawing.Bitmap(110,110)
$smallGraphics = [Drawing.Graphics]::FromImage($smallBackground)
try { $smallGraphics.Clear([Drawing.Color]::White); $smallGraphics.DrawImage($small,0,0,110,110); $smallBackground.Save((Join-Path $PSScriptRoot 'wizard-small.bmp'), [Drawing.Imaging.ImageFormat]::Bmp) } finally { $smallGraphics.Dispose(); $smallBackground.Dispose(); $small.Dispose() }
[BrandArt]::Wizard((Join-Path $PSScriptRoot 'wizard.bmp'))
