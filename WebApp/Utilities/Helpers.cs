using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;

namespace WebApp.Utilities
{
    public static class Helpers
    {
        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        /// <summary>
        /// Resize Image
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Image resizeImage(string stPhotoPath, int newWidth, int newHeight)
        {
            Image imgPhoto = Image.FromFile(stPhotoPath);

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;

            //Consider vertical pics
            if (sourceWidth < sourceHeight)
            {
                int buff = newWidth;

                newWidth = newHeight;
                newHeight = buff;
            }

            int sourceX = 0, sourceY = 0, destX = 0, destY = 0;
            float nPercent = 0, nPercentW = 0, nPercentH = 0;

            nPercentW = ((float)newWidth / (float)sourceWidth);
            nPercentH = ((float)newHeight / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((newWidth -
                          (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((newHeight -
                          (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);


            Bitmap bmPhoto = new Bitmap(newWidth, newHeight,
                          PixelFormat.Format24bppRgb);

            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                         imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Black);
            grPhoto.InterpolationMode =
                InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            imgPhoto.Dispose();
            return bmPhoto;
        }

        /// <summary>
        /// Returns an array containing the salt at index 0 and the password hash at index 1
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        internal static string[] GetHashedPassword(string password, string salt = null)
        {
            if (String.IsNullOrEmpty(salt))
            {
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                int SaltByteSize = 16;
                byte[] buff = new byte[SaltByteSize];
                rng.GetBytes(buff);
                salt = Convert.ToBase64String(buff);
            }
            string passwordHash = FormsAuthentication.HashPasswordForStoringInConfigFile(salt + password, "SHA1");

            return new string[]{
                salt,
                passwordHash
            };
        }

        public static bool IsLoggedIn(HttpContextBase context, out int userid)
        {
            Int32.TryParse((context.Session["UserId"] ?? string.Empty).ToString(), out userid);
            return context.Session["UserId"] != null;
        }

        public static bool IsAdminLoggedIn(HttpContextBase context, out int userid)
        {
            var groupid = 0;
            Int32.TryParse((context.Session["UserId"] ?? string.Empty).ToString(), out userid);
            Int32.TryParse((context.Session["UserGroupId"] ?? string.Empty).ToString(), out groupid);
            return context.Session["UserId"] != null && groupid == 1;
        }

        public static bool IsLoggedIn(HttpContextBase context)
        {
            return context.Session["UserId"] != null;
        }

        public static string GetRandomPassword()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        }
    }
}