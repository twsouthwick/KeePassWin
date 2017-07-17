using System;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

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
            ((ByteImage)d).Update((byte[])e.NewValue);
        }

        private async void Update(byte[] image)
        {
            if (image == null)
            {
                imageControl.Source = null;
                return;
            }

            var bitmap = new BitmapImage();

            using (var ms = new MemoryStream(image))
            {
                await bitmap.SetSourceAsync(ms.AsRandomAccessStream());
            }

            imageControl.Source = bitmap;
        }
    }
}
