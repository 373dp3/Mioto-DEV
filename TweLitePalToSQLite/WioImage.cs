// See https://aka.ms/new-console-template for more information
using System.Drawing;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:プラットフォームの互換性を検証", Justification = "<保留中>")]


public class WioImage
{
    public int width { get; set; } = 320;
    public int height { get; set; } = 240;
    public string fontName { get; set; } = "MS UI Gothic";
    public int shadowOffset { set; get; } = 2;
    public MemoryStream drawTempMonitor(string title, float temp, float humi, int tileHeight=40)
    {
        using (var canvas = new Bitmap(width, height))
        using (var g = Graphics.FromImage(canvas))
        {
            var posY = 0;
            var tmpHeight = 35;
            drawTextBox(title, width, tmpHeight, 0, 0, g, 
                Brushes.Black, Brushes.White, 20);
            posY += tmpHeight;
            tmpHeight = 149;
            if(temp > 30)
            {
                drawTextBox($"{temp:0.0}℃", width, tmpHeight, 0, posY, g,
                    Brushes.Red, Brushes.White, 200);
            }
            else
            {
                drawTextBox($"{temp:0.0}℃", width, tmpHeight, 0, posY, g,
                    Brushes.LightGreen, Brushes.DarkGreen, 200);
            }
            posY += tmpHeight;

            tmpHeight = 56;
            var wbgt = getWBGT(temp, humi);

            var msg = $"注意(湿度{humi:0}%)";
            var bgBrush = Brushes.SkyBlue;
            var txtBrush = Brushes.White;

            if ((wbgt>=25) && (wbgt < 28))
            {
                msg = $"警戒(湿度{humi:0}%)";
                bgBrush = Brushes.Yellow;
                txtBrush = Brushes.Black;
            }
            if ((wbgt >= 28) && (wbgt < 31))
            {
                msg = $"厳重警戒(湿度{humi:0}%)";
                bgBrush = Brushes.Orange;
                txtBrush = Brushes.Black;
            }
            if (wbgt >= 31)
            {
                msg = $"危険(湿度{humi:0}%)";
                bgBrush = Brushes.DarkViolet;
                txtBrush = Brushes.White;
            }


            drawTextBox(msg, width, tmpHeight, 0, posY, g, bgBrush, txtBrush, 25);
            posY += tmpHeight;


            var eps = new System.Drawing.Imaging.EncoderParameters(1);
            eps.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                System.Drawing.Imaging.Encoder.Quality, (long)80);
            var ici = GetEncoderInfo("image/jpeg");
            var mem = new MemoryStream(100 * 1024);
            canvas.Save(mem, ici, eps);
            return mem;
        }
    }

    /// <summary>
    /// 簡易的な暑さ指数を求める (屋内なのでSR=0、WS=0.1)
    /// https://blog.obniz.com/news/obniz-wbgt-service.html
    /// </summary>
    /// <param name="Ta">温度(℃)</param>
    /// <param name="RH">湿度(%)</param>
    /// <param name="SR">全天日射量(kW/m2</param>
    /// <param name="WS">風速(m/s)</param>
    /// <returns></returns>
    private float getWBGT(float Ta, float RH, float SR=0, float WS=0.1f)
    {
        return (float)(0.735 * Ta + 0.0374 * RH + 0.00292 * Ta * RH 
                + 7.619 * SR - 4.557 * SR - 0.0572 * WS - 4.064);
    }

    private void drawTextBox(string s, int titleWidth, int tileHeight, int posX, int posY, 
        Graphics g, Brush bgColor, Brush textColor, int initialFontSize=30)
    {
        var preHint = g.TextRenderingHint;
        g.FillRectangle(bgColor, posX, posY, titleWidth, tileHeight);

        var fontSize = fitFontSize(g, s, initialFontSize, width);
        if (fontSize > 10)
        {
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        }
        using (Font fnt = new Font(fontName, fontSize, FontStyle.Regular))
        using (StringFormat sf = new StringFormat())
        {
            SizeF stringSize = g.MeasureString(s, fnt, 1000, sf);
            var xgap = width - stringSize.Width;
            var ygap = tileHeight - stringSize.Height;
            if (fontSize > 30)
            {
                g.DrawString(s, fnt, new SolidBrush(Color.FromArgb(80, Color.Black)),
                    posX + shadowOffset + xgap / 2, posY + shadowOffset + ygap / 2, sf);
            }
            else
            {
                g.DrawString(s, fnt, new SolidBrush(Color.FromArgb(80, Color.Black)),
                    posX + 1 + xgap / 2, posY + 1 + ygap / 2, sf);
            }
            g.DrawString(s, fnt, textColor, posX + xgap / 2, posY + ygap / 2, sf);
        }
        g.TextRenderingHint = preHint;
    }

    private int fitFontSize(Graphics g, string s, int initialFontSize, int limitWidth)
    {
        for(int i= initialFontSize; i>=1; i--)
        {
            using (Font fnt = new Font(fontName, i, FontStyle.Regular))
            using (StringFormat sf = new StringFormat())
            {
                SizeF stringSize = g.MeasureString(s, fnt, 1000, sf);
                if (stringSize.Width > width) { continue; }
                return i;
            }

        }
        return 1;
    }

    //MimeTypeで指定されたImageCodecInfoを探して返す
    static System.Drawing.Imaging.ImageCodecInfo
        GetEncoderInfo(string mineType)
    {
        //GDI+ に組み込まれたイメージ エンコーダに関する情報をすべて取得
        System.Drawing.Imaging.ImageCodecInfo[] encs =
            System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
        //指定されたMimeTypeを探して見つかれば返す
        foreach (System.Drawing.Imaging.ImageCodecInfo enc in encs)
        {
            if (enc.MimeType == mineType)
            {
                return enc;
            }
        }
        return null;
    }
}




