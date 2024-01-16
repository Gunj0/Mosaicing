using System.Drawing.Drawing2D;

namespace Mosaicing
{
  public partial class Form1 : Form
  {
    private const int MOSAIC_SIZE = 10;
    private const int SAMPLING_SIZE = 10; // モザイクの四角の大きさ

    private List<string> imagePaths;
    private int currentImageIndex = 0;
    private bool isMouseDragging = false;
    private Rectangle selectionRectangle;

    public Form1()
    {
      InitializeComponent();

      // フォルダ選択
      if (SelectImageFolder())
      {
        // 画像読み込み
        DisplayCurrentImage();
        TopMost = true;
        // 画像切替
        pictureBox1.MouseWheel += PictureBox_MouseWheel;
        // マウス選択モザイク
        pictureBox1.MouseDown += PictureBox_MouseDown;
        pictureBox1.MouseMove += PictureBox_MouseMove;
        pictureBox1.MouseUp += PictureBox_MouseUp;
        pictureBox1.Paint += PictureBox_Paint;
      }
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
      // フォームがロードされた後の処理
      TopMost = true; // 例: フォームを手前に表示する
    }

    // フォルダ選択
    private bool SelectImageFolder()
    {
      using (var folderDialog = new FolderBrowserDialog())
      {
        folderDialog.Description = "フォルダ選択";
        folderDialog.ShowNewFolderButton = false;

        if (folderDialog.ShowDialog() == DialogResult.OK)
        {
          string selectedFolder = folderDialog.SelectedPath;
          InitializeImageList(selectedFolder);
          return true;
        }
        else
        {
          // ユーザーがキャンセルした場合
          return false;
        }
      }
    }

    // 画像読み込み
    private void InitializeImageList(string folderPath)
    {
      imagePaths = new List<string>();

      if (Directory.Exists(folderPath))
      {
        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        foreach (string filePath in Directory.GetFiles(folderPath))
        {
          if (Array.Exists(allowedExtensions, ext => ext.Equals(Path.GetExtension(filePath), StringComparison.OrdinalIgnoreCase)))
          {
            imagePaths.Add(filePath);
          }
        }
      }
    }
    private void DisplayCurrentImage()
    {
      if (imagePaths.Count > 0 && currentImageIndex >= 0 && currentImageIndex < imagePaths.Count)
      {
        pictureBox1.Image = Image.FromFile(imagePaths[currentImageIndex]);
      }
      SetFormTitleWithImageFileName();
    }
    private void SetFormTitleWithImageFileName()
    {
      if (currentImageIndex >= 0 && currentImageIndex < imagePaths.Count)
      {
        string fileName = Path.GetFileName(imagePaths[currentImageIndex]);
        Text = fileName; // フォームのタイトルに設定
      }
    }

    // 画像切替
    private void Form1_KeyDown(object sender, KeyEventArgs e)
    {
      switch (e.KeyCode)
      {
        case Keys.Left:
          currentImageIndex = (currentImageIndex - 1 + imagePaths.Count) % imagePaths.Count;
          DisplayCurrentImage();
          break;
        case Keys.Right:
          currentImageIndex = (currentImageIndex + 1) % imagePaths.Count;
          DisplayCurrentImage();
          break;
      }
      SetFormTitleWithImageFileName();
    }
    private void PictureBox_MouseWheel(object sender, MouseEventArgs e)
    {
      // マウスのホイールが回転したときの処理
      int delta = e.Delta;

      if (delta > 0)
      {
        // 上にスクロールした場合
        currentImageIndex = (currentImageIndex - 1 + imagePaths.Count) % imagePaths.Count;
      }
      else
      {
        // 下にスクロールした場合
        currentImageIndex = (currentImageIndex + 1) % imagePaths.Count;
      }

      DisplayCurrentImage();
      SetFormTitleWithImageFileName();
    }

    // マウス選択処理
    private void PictureBox_MouseDown(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left)
      {
        isMouseDragging = true;
        selectionRectangle = new Rectangle(e.Location, new Size(0, 0));
      }
    }
    private void PictureBox_MouseMove(object sender, MouseEventArgs e)
    {
      if (isMouseDragging)
      {
        selectionRectangle = new Rectangle(selectionRectangle.Location, new Size(e.X - selectionRectangle.X, e.Y - selectionRectangle.Y));
        pictureBox1.Invalidate(); // 再描画を要求して選択範囲を表示
      }
    }
    private void PictureBox_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left && isMouseDragging)
      {
        isMouseDragging = false;
        ApplyMosaicToSelection();
        pictureBox1.Invalidate(); // 再描画を要求して選択範囲を消す
      }
    }
    private void PictureBox_Paint(object sender, PaintEventArgs e)
    {
      if (isMouseDragging)
      {
        using (Pen pen = new Pen(Color.Red))
        {
          pen.DashStyle = DashStyle.Dash; // 赤い点線を設定
          e.Graphics.DrawRectangle(pen, selectionRectangle);
        }
      }
    }

    // モザイク処理
    private void ApplyMosaicToSelection()
    {
      if (pictureBox1.Image != null)
      {
        Bitmap bitmap = new Bitmap(pictureBox1.Image);

        for (int x = selectionRectangle.X; x < selectionRectangle.X + selectionRectangle.Width; x++)
        {
          for (int y = selectionRectangle.Y; y < selectionRectangle.Y + selectionRectangle.Height; y++)
          {
            ApplyAverageColor(bitmap, x, y);
          }
        }

        pictureBox1.Image = bitmap;

        // ファイル保存
        string filePath = imagePaths[currentImageIndex];
        string directory = Path.GetDirectoryName(filePath);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string fileExtension = Path.GetExtension(filePath);
        string newFilePath = Path.Combine(directory, fileNameWithoutExtension + "_mosaic" + fileExtension);
        int fileCount = 1;
        while (File.Exists(newFilePath))
        {
          newFilePath = Path.Combine(directory, fileNameWithoutExtension + "_mosaic_" + fileCount + fileExtension);
          fileCount++;
        }
        bitmap.Save(newFilePath);
      }
    }
    private void ApplyAverageColor(Bitmap bitmap, int startX, int startY)
    {
      int sumR = 0, sumG = 0, sumB = 0;
      int count = 0;

      for (int x = startX; x < startX + MOSAIC_SIZE && x < bitmap.Width; x++)
      {
        for (int y = startY; y < startY + MOSAIC_SIZE && y < bitmap.Height; y++)
        {
          // サンプリングサイズにより、より多くのピクセルをサンプリング
          if (x % SAMPLING_SIZE == 0 && y % SAMPLING_SIZE == 0)
          {
            Color pixel = bitmap.GetPixel(x, y);
            sumR += pixel.R;
            sumG += pixel.G;
            sumB += pixel.B;
            count++;
          }
        }
      }

      Color averageColor = (count > 0) ?
        Color.FromArgb(sumR / count, sumG / count, sumB / count)
        : Color.Black; // デフォルトの色を指定

      for (int x = startX; x < startX + MOSAIC_SIZE && x < bitmap.Width; x++)
      {
        for (int y = startY; y < startY + MOSAIC_SIZE && y < bitmap.Height; y++)
        {
          bitmap.SetPixel(x, y, averageColor);
        }
      }
    }

    private void pictureBox1_Click(object sender, EventArgs e)
    {

    }
  }
}
