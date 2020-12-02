using ChartJs.Blazor.ChartJS.Common.Axes;
using ChartJs.Blazor.ChartJS.Common.Axes.Ticks;
using ChartJs.Blazor.ChartJS.Common.Enums;
using ChartJs.Blazor.ChartJS.Common.Handlers;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.Common.Time;
using ChartJs.Blazor.ChartJS.LineChart;
using MiotoBlazorCommon.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiotoBlazorClient
{
    public class ChartJsGanttHelper
    {
        public string[] ColorPallet = new string[]{
            "#4472C4",
            "#ED7D31",
            "#A5A5A5",
            "#FFC000",
            "#5B9BD5",
            "#70AD47",
            "#264478",
            "#9E480E",
            "#636363",
            "#997300",
            "#255E91",
            "#43682B"
        };
        public string[] ColorPalletBackGround = new string[]{
            "#A1B8E1",
            "#F6BE98",
            "#D2D2D2",
            "#FFDF7F",
            "#ADCDEA",
            "#B7D6A3",
            "#92A1BB",
            "#CEA386",
            "#B1B1B1",
            "#CCB97F",
            "#92AEC8",
            "#A1B395"
        };

        public LineConfig config = new LineConfig();
        public LineConfig configShort = new LineConfig();

        public ChartJsGanttHelper(Config cfg, DateTime targetDt)
        {
            //ガントチャート描画開始・終了日時の設定
            var hh = (int)(cfg.dateLineHHMM / 100);
            var mm = cfg.dateLineHHMM - 100 * hh;
            var dtStart = DateTime.Parse(targetDt.ToString("yyyy/MM/dd") + $" {hh}:{mm}:00");
            //仮算出した職務日切り替え時刻の方が未来なら、切替日を1日過去方向にずらす。
            if(dtStart > targetDt) { dtStart = dtStart.AddDays(-1); }
            var dtEnd = dtStart.AddDays(1);

            //Graph configの設定
            config = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
                    Legend = new Legend
                    {
                        Display = false,
                        Position = Position.Right,
                        Labels = new LegendLabelConfiguration
                        {
                            UsePointStyle = true
                        }
                    },
                    Tooltips = new Tooltips
                    {
                        Mode = InteractionMode.Nearest,
                        Intersect = false,
                    },
                    Scales = new Scales
                    {
                        xAxes = new List<CartesianAxis>
                        {
                                new TimeAxis
                                {
                                    Distribution = TimeDistribution.Linear,
                                    Ticks = new TimeTicks
                                    {
                                        Source = TickSource.Auto,
                                    },
                                    Time = new TimeOptions
                                    {
                                        Unit = TimeMeasurement.Hour,
                                        Round = TimeMeasurement.Second,
                                        TooltipFormat = "MM/DD HH:mm:ss",
                                        DisplayFormats = TimeDisplayFormats.DE_CH,
                                        Max = new Moment(dtEnd),
                                        Min = new Moment(dtStart),
                                    },
                                    ScaleLabel = new ScaleLabel
                                    {
                                        Display = false
                                    },
                                },
                        },
                        yAxes = new List<CartesianAxis>
                        {
                            new LinearCartesianAxis
                            {
                                Ticks = new LinearCartesianTicks
                                {
                                    Max = 1,
                                    Min = 0
                                },
                                Display = AxisDisplay.False,

                            }

                        }
                    },
                    /*                    Hover = new LineOptionsHover
                                        {
                                            Intersect = true,
                                            Mode = InteractionMode.Y
                                        }*/
                }
            };


            //短いガントチャートconfigの設定
            configShort = new LineConfig
            {
                Options = new LineOptions
                {
                    Responsive = true,
                    Legend = new Legend
                    {
                        Display = false,
                        Position = Position.Right,
                        Labels = new LegendLabelConfiguration
                        {
                            UsePointStyle = true
                        }
                    },
                    Tooltips = new Tooltips
                    {
                        Mode = InteractionMode.Nearest,
                        Intersect = false,
                    },
                    Scales = new Scales
                    {
                        xAxes = new List<CartesianAxis>
                        {
                                new TimeAxis
                                {
                                    Distribution = TimeDistribution.Linear,
                                    Ticks = new TimeTicks
                                    {
                                        Source = TickSource.Auto,
                                    },
                                    Time = new TimeOptions
                                    {
                                        Unit = TimeMeasurement.Minute,
                                        Round = TimeMeasurement.Second,
                                        TooltipFormat = "MM/DD HH:mm:ss",
                                        DisplayFormats = TimeDisplayFormats.DE_CH,
                                    },
                                    ScaleLabel = new ScaleLabel
                                    {
                                        Display = false
                                    },
                                },
                        },
                        yAxes = new List<CartesianAxis>
                        {
                            new LinearCartesianAxis
                            {
                                Ticks = new LinearCartesianTicks
                                {
                                    Max = 1,
                                    Min = 0
                                },
                                Display = AxisDisplay.False,

                            }

                        }
                    },
                }
            };



            int cnt = config.Data.Datasets.Count;
            lineDataSet = new LineDataset<TimeTuple<double>>
            {
                BackgroundColor = ColorPalletBackGround[(cnt) % ColorPalletBackGround.Length],
                BorderColor = ColorPallet[(cnt) % ColorPallet.Length],
                Fill = true,
                BorderWidth = 0,
                PointRadius = 0,
                PointBorderWidth = 0,
                SteppedLine = SteppedLine.True
            };
            config.Data.Datasets.Add(lineDataSet);

            lineDataSetShort = new LineDataset<TimeTuple<double>>
            {
                BackgroundColor = ColorPalletBackGround[(cnt + 1) % ColorPalletBackGround.Length],
                BorderColor = ColorPallet[(cnt + 1) % ColorPallet.Length],
                Fill = true,
                BorderWidth = 0,
                PointRadius = 0,
                PointBorderWidth = 0,
                SteppedLine = SteppedLine.True
            };
            configShort.Data.Datasets.Add(lineDataSetShort);
        }

        private LineDataset<TimeTuple<double>> lineDataSet;
        private LineDataset<TimeTuple<double>> lineDataSetShort;
        private int shortDurationHour = -2;

        private DateTime maxDt = DateTime.Parse("1970/1/1");

        public void createLine(List<(DateTime, double)> dataset)
        {
            if (dataset == null) { return; }
            if (dataset.Count == 0) { return; }

            var ds = dataset.Where(q => q.Item1 > maxDt)
                            .Select(q => new TimeTuple<double>(new Moment(q.Item1), q.Item2));
            lineDataSet.AddRange(ds);

            //短いガントチャート用
            {
                var dss = dataset.Where(q => q.Item1 > maxDt)
                .Select(q => new TimeTuple<double>(new Moment(q.Item1), q.Item2));
                lineDataSetShort.AddRange(dss);

                maxDt = dataset.LastOrDefault().Item1;
                var minDt = maxDt.AddHours(shortDurationHour);//-2

                //表示対象外のデータを削除
                var expireSet = lineDataSetShort.Data.Where(q => (DateTime)q.Time < minDt);
                var expireMax = expireSet.OrderBy(q => (DateTime)q.Time).LastOrDefault();
                foreach (var item in expireSet)
                {
                    if(item == expireMax) { continue; }//表示エリアまたぎ用のデータを削除しないように
                    //lineDataSetShort.Remove(item);
                }
                var axis = (TimeAxis)configShort.Options.Scales.xAxes[0];
                
                axis.Time.Max = new Moment(maxDt);
                axis.Time.Min = new Moment(minDt);//余裕を元に戻す
            }


        }

        public void clearAllLine()
        {
            lineDataSet.RemoveRange(0, lineDataSet.Data.Count);

        }
    }
}
