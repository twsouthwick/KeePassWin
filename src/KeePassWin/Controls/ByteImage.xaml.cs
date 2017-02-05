using System;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KeePass.Win.Controls
{
    public sealed partial class ByteImage : UserControl
    {
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(byte[]), typeof(ByteImage), new PropertyMetadata(null, ImageUpdate));

        public ByteImage()
        {
            this.InitializeComponent();
        }

        public byte[] Image
        {
            get { return (byte[])GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        private static void ImageUpdate(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ByteImage)d).Update();
        }

        private async void Update()
        {
            var image = Image;

            if (image == null)
            {
                return;
            }

            using (var ms = new MemoryStream(image))
            {
                await BitmapImageSource.SetSourceAsync(ms.AsRandomAccessStream());
            }
        }
    }
}
