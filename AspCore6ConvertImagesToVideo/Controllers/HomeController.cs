using Microsoft.AspNetCore.Mvc;
using SharpAvi;
using SharpAvi.Output;
using System.Drawing;

namespace AspCore6ConvertImagesToVideo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment webHostEnvironment;

        public HomeController(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> upload(List<IFormFile> files)
        {
            try
            {
                Bitmap thisBitmap;

                var uploadpath = $"{webHostEnvironment.WebRootPath}/Videos/{Guid.NewGuid()}.avi";

                //creates the writer of the file (to save the video)
                var writer = new AviWriter(uploadpath)
                {
                    FramesPerSecond = 1,
                    EmitIndex1 = true
                };


                using var ms1 = new MemoryStream();
                await files[0].CopyToAsync(ms1);

                using var img1 = Image.FromStream(ms1);
                thisBitmap = new Bitmap(img1);

                int width = thisBitmap.Width;
                int height = thisBitmap.Height;


                var stream = writer.AddVideoStream();
                stream.Width = width;
                stream.Height = height;
                stream.Codec = CodecIds.Uncompressed;
                stream.BitsPerPixel = BitsPerPixel.Bpp32;


                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await file.CopyToAsync(ms);

                        using var img = Image.FromStream(ms);


                        thisBitmap = new Bitmap(img);

                        //convert the bitmap to a byte array
                        byte[] byteArray = BitmapToByteArray(thisBitmap);


                        byte[] Header = byteArray.Take(54).ToArray();
                        byte[] picture = byteArray.Skip(54).ToArray();

                        stream.WriteFrame(false, picture, 0, picture.Length);
                    }
                }
                writer.Close();
            }
            catch (Exception ex)
            {
            }

            return RedirectToAction("Index");
        }

        [NonAction]
        public static byte[] BitmapToByteArray(Image img)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                stream.Close();

                byteArray = stream.ToArray();
            }
            return byteArray;
        }
    }
}