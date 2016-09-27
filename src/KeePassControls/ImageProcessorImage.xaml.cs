using System;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ImageProcessorCore.Formats;

using IPImage = ImageProcessorCore.Image;

namespace KeePass.Controls
{
    public sealed partial class ImageProcessorImage : UserControl
    {
        private static readonly IImageEncoder s_encoder = new PngEncoder();

        public ImageProcessorImage()
        {
            this.InitializeComponent();
        }

        public IPImage Image
        {
            get { return (IPImage)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(IPImage), typeof(ImageProcessorImage), new PropertyMetadata(null, ImageUpdate));

        private static void ImageUpdate(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ImageProcessorImage)d).Update();
        }

        private async void Update()
        {
            var image = Image;

            if (image == null)
            {
                return;
            }

            using (var ms = new MemoryStream())
            {
                image.Save(ms, s_encoder);
                ms.Flush();
                ms.Position = 0;

                await BitmapImageSource.SetSourceAsync(ms.AsRandomAccessStream());
            }
        }
    }
}
