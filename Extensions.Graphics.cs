using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace KCS.Common.Shared
{
	/// <summary>
	/// Utility methods for graphics.
	/// </summary>
	public static class GraphicsExtensions
	{
        private static Dictionary<string, Color> _htmlColorMap = null;
		private static List<string> _imageExtensions = null;
		private static object _Lock = new object();

        /// <summary>
        /// Created by 9OPARA7. Contains a dictionary of colors keyed by Hex Code.
        /// </summary>
        public static Dictionary<string, Color> HtmlColorMap
        {
            get
            {
                string hexCode;
                if (_htmlColorMap == null)
                {
                    lock (_Lock)
                    {
                        _htmlColorMap = new Dictionary<string, Color>(138);
                        foreach (Color c in GetNamedColors(true, false))
                        {
                            hexCode = c.GetHexCode(true);
                            if (!_htmlColorMap.ContainsKey(hexCode))
                            {
                                _htmlColorMap.Add(hexCode, c);
                            }
                        }
                    }
                }
                return _htmlColorMap;
            }
        }

		/// <summary>
		/// Created by 9OPARA7. Contains the list of valid image extensions.
		/// </summary>
		public static List<string> ImageExtensions
		{
			get
			{
				if (_imageExtensions == null)
				{
					lock (_Lock)
					{
						_imageExtensions = new List<string>()
						{
							".bmp", ".gif", ".ico", ".jpeg", ".jpg", ".png", ".tiff", ".tif", ".ai", ".pdf"
						};
					}
				}
				return _imageExtensions;
			}
		}        

        /// <summary>
        /// Created by 9OPARA7. Gets all Named colors, including "Transparent". That is, Known colors that are NOT System colors, sorted by hue.
        /// 
        /// </summary>
        /// <returns>IEnumerable of Colors.</returns>
        /// <remarks>This method ignores the Transparent color.</remarks>
        public static IEnumerable<Color> GetNamedColors()
        {
            return GetNamedColors(true, true);
        }

        /// <summary>
        /// Created by 9OPARA7. Gets all Named colors. That is, Known colors that are NOT System colors.
        /// </summary>
        /// <param name="sortByShade">If TRUE, the results are sorted by shade. Else, they are sorted by name.</param>
        /// <param name="includeTransparent">If true, includes the "transparent" color.</param>
        /// <returns>IEnumerable of Colors.</returns>
        public static IEnumerable<Color> GetNamedColors(bool sortByShade, bool includeTransparent)
        {
            Color c;
            Array colors = Enum.GetValues(typeof(KnownColor));
            List<Color> list = new List<Color>();

            foreach (KnownColor kc in colors)
            {
                c = Color.FromKnownColor(kc);
                if (!c.IsSystemColor /*&& c != Color.Transparent*/)
                {
                    list.Add(c);
                }
            }

            // Remove the transparent color if necessary.
            if (!includeTransparent)
            {
                list.Remove(Color.Transparent);
            }

            // If we are sorting by shade, do it here.
            if (sortByShade)
            {
                var query = from color in list
                            //orderby string.Format("{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B) descending
                            orderby color.GetHexCode() descending
                            select color;
                list = query.ToList();
            }
            return list;
        }

        /// <summary>
        /// Get a snapshot of a screen.
        /// </summary>
        /// <param name="screen">Screen to capture.</param>
        /// <param name="width">Desired width.</param>
        /// <param name="height">Desired height.</param>
        /// <returns>Captured image.</returns>
        public static Image GetScreenshot(this Screen screen, int width, int height)
        {
            Image imageThumbNail;
            Graphics graphicsThumbNail = null;

            Image image = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(image);

            try
            {
                graphics.CopyFromScreen(screen.Bounds.Location, new Point(0, 0), image.Size);

                // Create the target image
                imageThumbNail = new Bitmap(width, height);
                graphicsThumbNail = Graphics.FromImage(imageThumbNail);
                graphicsThumbNail.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphicsThumbNail.DrawImage(image, 0, 0, width, height);
                image = imageThumbNail;
            }
            finally
            {
                if (graphics != null)
                {
                    graphics.Dispose();
                }
            }
            return image;
        }

        /// <summary>
        /// Get a resized (nicely resampled) snapshot of a Screen.
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="resizeFactor">Percent by which to adjust the size. 100 means no resize.</param>
        /// <returns></returns>
        public static Image GetScreenshot(this Screen screen, byte resizeFactor)
        {
            float width = screen.Bounds.Width * (float)(resizeFactor / 100F);
            float height = screen.Bounds.Height * (float)(resizeFactor / 100F);
            return GetScreenshot(screen, Convert.ToInt32(width), Convert.ToInt32(height));
        }

        /// <summary>
        /// Get a full snapshot of a Screen.
        /// </summary>
        /// <param name="screen"></param>
        /// <returns></returns>
        public static Image GetScreenshot(this Screen screen)
        {
            return GetScreenshot(screen, 100);
        }

		/// <summary>
		/// Created by 9OPARA7. Checks to see if a file is really an image.
		/// </summary>
		/// <param name="filename"></param>
		/// <returns>TRUE if the given filename points to a valid image.</returns>
		/// <remarks>
		/// This "quick test" loads the entire image into memory. Find a way to do this more efficiently
		/// </remarks>
		public static bool IsImageFile(string filename)
		{
			string ext;
			//Image img = null;
			bool isValid;
			FileStream stream = null;

			// Check the extension first
			ext = Path.GetExtension(filename);
			if (!ImageExtensions.Contains(ext.ToLower()))
			{
				return false;
			}

			// Read the image without validating image data 
			try
			{
                if (ext.ToLower() == ".ai" || ext.ToLower() == ".pdf")
                {
                    isValid = true;
                }
                else
                {
                    stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    using (Image img = Image.FromStream(stream, false, false))
                    {
                        isValid = true;
                    }
                }

				return isValid;
			}
			catch
			{
				return false;
			}
			finally
			{
				if (stream != null)
				{
					stream.Dispose();
					stream = null;
				}
			}
		}

        /// <summary>
        /// Converts an image to raw binary data.
        /// </summary>
        /// <param name="image">Image to convert.</param>
        /// <param name="format">Image format.</param>
        /// <returns>The raw bytes of the image, or null.</returns>
        public static byte[] GetBytes(this Image image, ImageFormat format)
        {
            byte[] data = null;

            ImageConverter converter = new ImageConverter();
            data = (byte[])converter.ConvertTo(image, typeof(byte[]));

            return data;
        }

        /// <summary>
        /// Converts an image to Postscript.
        /// </summary>
        /// <param name="imageFilePath">Path to image that will be converted.</param>
        /// <returns>Postscript data as a byte array.</returns>
        public static byte[] GetPostScript(string imageFilePath)
        {
            Bitmap bmp = new Bitmap(imageFilePath);
            string fileName = Path.GetFileName(imageFilePath);
            return GetPostScript(bmp, fileName);
        }

        public static byte[] GetPostScript(this Image image)
        {
            return GetPostScript(image, "Image");
        }

        /// <summary>
        /// Converts an image to Postscript.
        /// </summary>
        /// <param name="bitmap">Bitmap to convert.</param>
        /// <param name="imageName">Name of the image to be embedded. Any string is fine. Empty string will be defaulted to "Image".</param>
        /// <returns>Postscript data as a byte array.</returns>
        public static byte[] GetPostScript(this Image image, string imageName)
        {
            string path = Path.GetTempFileName();
            if (string.IsNullOrWhiteSpace(imageName))
            {
                imageName = "Image";
            }

            Bitmap bitmap = new Bitmap(image);

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("%!PS-Adobe-3.0");
                //sw.WriteLine("25 500 translate");
                // sw.WriteLine(string.Format("{0} {1} scale", originalImage.Width, originalImage.Height));
                sw.WriteLine("/" + imageName + "{");
                sw.WriteLine(string.Format("{0} {1} 8", bitmap.Width, bitmap.Height));
                sw.WriteLine(string.Format("[{0} 0 0 {1} 0 {2}]", bitmap.Width, -bitmap.Height, bitmap.Height));
                sw.WriteLine("{\n<");

                BitmapData data = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppRgb);

                // Get the address of the first line.
                IntPtr ptr = data.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                int bytes = data.Stride * bitmap.Height;
                byte[] rgbValues = new byte[bytes];

                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                // Unlock the bits.
                bitmap.UnlockBits(data);

                int lwidth = 0;
                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Color clr = bitmap.GetPixel(x, y);
                        string str = string.Format("{0}{1}{2}",
                            clr.R.ToString("X").PadLeft(2, '0'),
                            clr.G.ToString("X").PadLeft(2, '0'),
                            clr.B.ToString("X").PadLeft(2, '0'));
                        sw.Write(str);

                        lwidth += 2 * 3;
                        if (lwidth >= 90)
                        {
                            sw.WriteLine("");
                            lwidth = 0;
                        }
                    }
                }

                /*
                int lwidth = 0;
                for (int y = 0; y < rgbValues.Length; y += 4)
                {
                    string str = string.Format("{0}{1}{2}", 
                        rgbValues[y+3].ToString("X"),
                        rgbValues[y+2].ToString("X"),
                        rgbValues[y+1].ToString("X"));
                    sw.Write(str);

                    lwidth += 2 * 3;
                    if (lwidth >= 90)
                    {
                        sw.WriteLine("");
                        lwidth = 0;
                    }
                }
                */

                sw.WriteLine(">\n}");
                sw.WriteLine("false 3 colorimage");
                sw.WriteLine("} bind def");
                sw.Close();                                
            }

            // Return the file as a byte array, and delete the original file.
            bool deleted;
            return ShellIO.GetFileBytes(path, true, out deleted);
        }

        /// <summary>
        /// Gets the appropriate extension of an image based on the raw format.
        /// </summary>
        /// <param name="imageData">Data to analyze.</param>
        /// <returns>Extension.</returns>
        public static string GetFileExtension(byte[] imageData)
        {
            MemoryStream ms = new MemoryStream(imageData);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
            try
            {
                return GetFileExtension(image);
            }
            finally
            {
                image.Dispose();
            }
        }

        /// <summary>
        /// Gets the appropriate extension of an image.
        /// </summary>
        /// <param name="image">Image to analyze.</param>
        /// <returns>Extension.</returns>
        public static string GetFileExtension(this Image image)
        {
            if (image.RawFormat.Equals(ImageFormat.Bmp)) return ".Bmp";
            if (image.RawFormat.Equals(ImageFormat.Emf)) return ".Emf";
            if (image.RawFormat.Equals(ImageFormat.Exif)) return ".Exif";
            if (image.RawFormat.Equals(ImageFormat.Gif)) return ".Gif";
            if (image.RawFormat.Equals(ImageFormat.Icon)) return ".Ico";
            if (image.RawFormat.Equals(ImageFormat.Jpeg)) return ".Jpeg";
            if (image.RawFormat.Equals(ImageFormat.MemoryBmp)) return ".bmp";
            if (image.RawFormat.Equals(ImageFormat.Png)) return ".Png";
            if (image.RawFormat.Equals(ImageFormat.Tiff)) return ".Tiff";
            if (image.RawFormat.Equals(ImageFormat.Wmf)) return ".wmf";

            image.Dispose();
            return string.Empty;
        }

        /// <summary>
        /// Converts a raw binary data back to an Image.
        /// </summary>
        /// <param name="data">Data to convert.</param>
        /// <returns>An Image, or null.</returns>
        public static Image GetImage(this byte[] data)
        {
            Image image = null;

            try
            {
                using (MemoryStream stream = new MemoryStream(data, 0, data.Length))
                {
                    stream.Write(data, 0, data.Length);
                    image = new Bitmap(Image.FromStream(stream, true));
                }
            }
            catch
            {
            }
            return image;
        }

        public static Bitmap GetReverseComboBoxButtonImage(Size size, System.Windows.Forms.VisualStyles.ComboBoxState btnState)
        {
            Bitmap bm = new Bitmap(size.Width, size.Height);
            Graphics g = Graphics.FromImage(bm);
            Rectangle rect = new Rectangle(0, 0, size.Width, size.Height);
            if (ComboBoxRenderer.IsSupported)
            {
                ComboBoxRenderer.DrawDropDownButton(g, rect, btnState);
            }
            else
            {
                ControlPaint.DrawComboButton(g, rect, ButtonState.Normal);
            }
            
            //e.Inner.Button.DrawButton(g, new Rectangle(0, 0, rect.Width, rect.Height), btnState, e.Inner.Style);
            bm.RotateFlip(RotateFlipType.Rotate180FlipNone);
            //graphics.DrawImage(bm, new Point(0, 0));
            //Bitmap ret = (Bitmap)bm.Clone();
            g.Dispose();
            return bm;
        }

		/// <summary>
		/// Attempts to translate a string to a Color.
		/// </summary>
		/// <param name="colorValue">String containing [what we hope is] a color value.</param>
		/// <returns>If unable to translate, returns Transparent.</returns>
		public static Color InterpreteColor(string colorValue)
		{
            if (string.IsNullOrEmpty(colorValue))
            {
                return Color.Transparent;
            }
			// Attempt to interprete get the color from the raw value, if we're so lucky
			Color color = Color.FromName(colorValue);

			// If this isn't a known color, maybe user entered a Hex code? As IF!! It's worth a shot though
			if (!color.IsKnownColor)
			{
				if (colorValue.Length == 6 && Strings.IsValidHexNumber(colorValue))
				{
					// Make sure string starts with hash sign.
					if (!colorValue.StartsWith("#"))
					{
						colorValue = "#" + colorValue;
					}
					try
					{
						colorValue = colorValue.ToUpper();
						color = ColorTranslator.FromHtml(colorValue);
					}
					catch		// Ignore the exception, even though it's expensive :(
					{
						color = Color.Transparent;
					}
				}
				else
				{
					color = Color.Transparent;
				}
			}

			return color;
		}

		public static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
		{

			GraphicsPath roundedRect = new GraphicsPath();
			roundedRect.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
			roundedRect.AddLine(rect.X + cornerRadius, rect.Y, rect.Right - cornerRadius * 2, rect.Y);
			roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
			roundedRect.AddLine(rect.Right, rect.Y + cornerRadius * 2, rect.Right, rect.Y + rect.Height - cornerRadius * 2);

			roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);

			roundedRect.AddLine(rect.Right - cornerRadius * 2, rect.Bottom, rect.X + cornerRadius * 2, rect.Bottom);
			roundedRect.AddArc(rect.X, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
			roundedRect.AddLine(rect.X, rect.Bottom - cornerRadius * 2, rect.X, rect.Y + cornerRadius * 2);
			roundedRect.CloseFigure();
			return roundedRect;
		}                

		/// <summary>
		/// Proportionately resample an image (from a file) to fit the necessary dimensions.
		/// </summary>
		/// <param name="path">Path to original image.</param>
		/// <param name="destSize">New size.</param>
		/// <returns>Resampled bitmap.</returns>
		public static System.Drawing.Image ResizeImage(string path, Size destSize)
		{
			System.Drawing.Image img = System.Drawing.Image.FromFile(path);
			if (destSize.Height == 0) destSize.Height = img.Height;
			if (destSize.Width == 0) destSize.Width = img.Width;
            try
            {
                return ResizeImage(img, destSize);
            }
            finally
            {
                img.Dispose();
				img = null;
            }
		}

        /// <summary>
        /// Proportionately resample an image (from a file) to fit the necessary dimensions.
        /// </summary>
        /// <param name="pictureData">Image data.</param>
        /// <param name="destSize">New size.</param>
        /// <returns>Resampled bitmap.</returns>
        public static System.Drawing.Image ResizeImage(byte[] pictureData, Size destSize)
        {
            MemoryStream ms = new MemoryStream(pictureData);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            return ResizeImage(img, destSize);
        }

		/// <summary>
		/// Proportionately resample an image to fit the necessary dimensions.
		/// </summary>
		/// <param name="img">Original image.</param>
		/// <param name="destSize">New size.</param>
		/// <returns>Resampled image.</returns>
		public static System.Drawing.Image ResizeImage(System.Drawing.Image img, Size destSize)
		{
			// Be sure to scale the original image
			bool resample = false;
			double bmpWidth = img.Size.Width, bmpHeight = img.Size.Height;
			int destWidth = destSize.Width, destHeight = destSize.Height;
			double scaleFactorWidth, scaleFactorHeight, scaleFactor;
			Bitmap destination = null;
			System.Drawing.Graphics gfx = null;

			// If the height and width of the bitmap fit in the target, return unchanged
			if (bmpWidth <= destWidth && bmpHeight <= destHeight) 
			{
				return new Bitmap(img);
			}

			// If the source image is larger than the target size, resize and set the resample flag
			if (bmpWidth > destWidth || bmpHeight > destHeight)
			{
				scaleFactorWidth = destWidth/bmpWidth;
				scaleFactorHeight = destHeight/bmpHeight;
				scaleFactor = scaleFactorWidth >= scaleFactorHeight ? scaleFactorHeight  : scaleFactorWidth;

				destWidth = Convert.ToInt32(bmpWidth * scaleFactor);
				destHeight = Convert.ToInt32(bmpHeight * scaleFactor);

				resample = true;
			}

			// If the source image is smaller than the target, make target size smaller
			if (bmpWidth < destWidth || bmpHeight < destHeight)
			{
				destWidth = Convert.ToInt32(bmpWidth);
				destHeight = Convert.ToInt32(bmpHeight);
			}

			// If there was a size change, then redraw image. Else, leave unchanged.
			destination = new Bitmap(destWidth, destHeight);
			gfx = System.Drawing.Graphics.FromImage(destination);

			// If we need to resample the image, do so.
			if (resample)
			{
				gfx.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                //TODO add anti-aliasing
			}

			gfx.DrawImage(img, 0, 0, destWidth, destHeight);
			gfx.Dispose();
			gfx = null;
			
			return (Image)destination;
		}

        public static Point FitRectangle(this Rectangle host, Size size)
        {
            int left = host.Left, top = host.Top;
            if (size.Width < host.Size.Width)
            {
                left += ((host.Size.Width - size.Width)/2);
            }

            if (size.Height < host.Size.Height)
            {
                top += ((host.Size.Height - size.Height)/2);
                top--;
            }

            return new Point(left, top);
        }
	              
        /// <summary>
        /// Returns an Image from Byte[]
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Image GetImageFromByte( byte[] bytes )
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return System.Drawing.Image.FromStream(ms);
            }
        }
    }
}
