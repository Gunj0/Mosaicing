using System.Drawing.Drawing2D;

namespace Mosaicing
{
  public partial class Form1 : Form
  {
    private const int MOSAIC_SIZE = 10;
    private const int SAMPLING_SIZE = 10; // ���U�C�N�̎l�p�̑傫��

    private List<string> imagePaths;
    private int currentImageIndex = 0;
    private bool isMouseDragging = false;
    private Rectangle selectionRectangle;

    public Form1()
    {
      InitializeComponent();

      // �t�H���_�I��
      if (SelectImageFolder())
      {
        // �摜�ǂݍ���
        DisplayCurrentImage();
        TopMost = true;
        // �摜�ؑ�
        pictureBox1.MouseWheel += PictureBox_MouseWheel;
        // �}�E�X�I�����U�C�N
        pictureBox1.MouseDown += PictureBox_MouseDown;
        pictureBox1.MouseMove += PictureBox_MouseMove;
        pictureBox1.MouseUp += PictureBox_MouseUp;
        pictureBox1.Paint += PictureBox_Paint;
      }
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
      // �t�H�[�������[�h���ꂽ��̏���
      TopMost = true; // ��: �t�H�[������O�ɕ\������
    }

    // �t�H���_�I��
    private bool SelectImageFolder()
    {
      using (var folderDialog = new FolderBrowserDialog())
      {
        folderDialog.Description = "�t�H���_�I��";
        folderDialog.ShowNewFolderButton = false;

        if (folderDialog.ShowDialog() == DialogResult.OK)
        {
          string selectedFolder = folderDialog.SelectedPath;
          InitializeImageList(selectedFolder);
          return true;
        }
        else
        {
          // ���[�U�[���L�����Z�������ꍇ
          return false;
        }
      }
    }

    // �摜�ǂݍ���
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
        Text = fileName; // �t�H�[���̃^�C�g���ɐݒ�
      }
    }

    // �摜�ؑ�
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
      // �}�E�X�̃z�C�[������]�����Ƃ��̏���
      int delta = e.Delta;

      if (delta > 0)
      {
        // ��ɃX�N���[�������ꍇ
        currentImageIndex = (currentImageIndex - 1 + imagePaths.Count) % imagePaths.Count;
      }
      else
      {
        // ���ɃX�N���[�������ꍇ
        currentImageIndex = (currentImageIndex + 1) % imagePaths.Count;
      }

      DisplayCurrentImage();
      SetFormTitleWithImageFileName();
    }

    // �}�E�X�I������
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
        pictureBox1.Invalidate(); // �ĕ`���v�����đI��͈͂�\��
      }
    }
    private void PictureBox_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left && isMouseDragging)
      {
        isMouseDragging = false;
        ApplyMosaicToSelection();
        pictureBox1.Invalidate(); // �ĕ`���v�����đI��͈͂�����
      }
    }
    private void PictureBox_Paint(object sender, PaintEventArgs e)
    {
      if (isMouseDragging)
      {
        using (Pen pen = new Pen(Color.Red))
        {
          pen.DashStyle = DashStyle.Dash; // �Ԃ��_����ݒ�
          e.Graphics.DrawRectangle(pen, selectionRectangle);
        }
      }
    }

    // ���U�C�N����
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

        // �t�@�C���ۑ�
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
          // �T���v�����O�T�C�Y�ɂ��A��葽���̃s�N�Z�����T���v�����O
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
        : Color.Black; // �f�t�H���g�̐F���w��

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
