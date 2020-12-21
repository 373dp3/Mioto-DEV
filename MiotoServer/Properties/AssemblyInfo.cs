using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// アセンブリに関する一般情報は以下の属性セットをとおして制御されます。
// アセンブリに関連付けられている情報を変更するには、
// これらの属性値を変更してください。
[assembly: AssemblyTitle("MiotoServer")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("dp3 Toshiaki MINAMI.")]
[assembly: AssemblyProduct("MiotoServer")]
[assembly: AssemblyCopyright("Copyright © dp3 Toshiaki MINAMI. 2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// ComVisible を false に設定すると、その型はこのアセンブリ内で COM コンポーネントから 
// 参照不可能になります。COM からこのアセンブリ内の型にアクセスする場合は、
// その型の ComVisible 属性を true に設定してください。
[assembly: ComVisible(false)]

// このプロジェクトが COM に公開される場合、次の GUID が typelib の ID になります
[assembly: Guid("4cca0a31-6fc0-4e9f-b9e6-e2b29c1581c3")]

// アセンブリのバージョン情報は次の 4 つの値で構成されています:
//
//      メジャー バージョン
//      マイナー バージョン
//      ビルド番号
//      Revision
//
// すべての値を指定するか、下のように '*' を使ってビルドおよびリビジョン番号を 
// 既定値にすることができます:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.4.3")]
[assembly: AssemblyFileVersion("1.4.3")]
[assembly: AssemblyInformationalVersion("1.4.3")]
/*
 * 2020.11.25
 * ・ガントチャート高さ変更機能の追加
 * 
 * 2020.11.25
 * ・TWE-Lite標準FW＋1s設定による稼働情報に対応
 * ・負論理対応
 * ・ガントチャート対応
 * ・DBの破棄漏れ修正
 * 
 * 2020.9.30
 * ・3つ以上のCOMポート対応
 * 
 * 2020.9.15
 * ・信号ON時に可動率が低下する処理を修正
 * ・信号ON状態でCT無指定要因登録を開始した場合に停止時間、稼働時間が極大値になるバグの修正
 * ・80ポート以外のポートでの動作対応
 * ・日付変更時にPollingが暴走するバグの修正
 * 
 * 2020.9.9
 * ・設備毎の標準CT、取数登録対応
 * ・稼働日切り替わり時にブラウザの強制リロード
 * ・アンドンページのフォントサイズ切り替え対応
 * ・最終信号日時更新対応
 * ・CSVフォーマットの更新(取数考慮済みの出来高、動作回数の追加)
 * ・http://localhost/mac2machine にて、MAC(16進表記),マシン名取得対応
 * ・SQLite日付切替時の例外発生対応
 * 
 * 2020.8.25
 * ・CTつき登録において品番、担当者登録欄を追加
 * ・CT未登録生産において、信号未受診時の色をグレーに修正
 * 
 * 2020.7.21
 * ・MaxTicksが遡る場合があるため、Blazor側で値が大きくなるときのみ更新するように対処療法を実施
 * 
 * 2020.7.14
 * ・通信方式をWebSocketからLongPollingに変更(ブラウザ20ページでの同時更新動作確認)
 * ・Raspberry Pi4での動作確認
 * ・最終CTを表示に追加
 * ・ブラウザ上でのメモリリーク改善
 * ・日付ダイアログをキャンセルしても適用されたバグの修正
 * 
 * 2020.6.22
 * ・子機追加不能バグ修正
 * ・CT無指定時の表示分割改善
 * 
 * 2020.6.17
 * ・CT指定なし処理への対応
 * ・CSV出力範囲指定対応
 * 
 * 2020.6.14
 * ・Blazor WebAssembly対応初版
 * 
 * 2020.5.11
 * ・停止・再開時にfinal、tNNNs指定ができない問題の対応
 * ・電流センサ向け「1」から始まるMACアドレス対応
 * ・mem:CT10へfinal、macフィルタ対応
 * ・バックアップフォルダ変更時にテキストボックスが反映されない問題への対応
 * 
 * 2020.3.19   1.3.2
 * ・sw is null 対応
 * ・Ah対応 http://localhost/ah/
 * 
 * 2020.3.10   1.3.1
 * ・OutOfRangeException対応
 * 
 * 2020.2.18   1.3.0
 * ・電流センサからの稼働情報対応、一部sqlite-net対応
 * 
 * 2020.1.9   1.2.8
 * ・Serial、電流センサ、メモリDB対応
 * 
 * 2019.12.4   1.2.7
 * ・yyyy化の漏れ対応
 * 
 * 2019.11.26   1.2.6
 * ・seqのdiff化
 * ・タイトルにverを表示
 * 
 * 2019.11.25   1.2.5
 * ・3日でタスクが終了する問題の回避(TaskScheduler 1.1 Type libraryの使用)
 * 
 * 2019.11.19   1.2.4
 * ・プレス機向けSingle Edge子機対応(btn 00のみ)
 * 
 * 2019.11.7    1.2.3
 * ・mp3ファイル複数再生対応。 http:// -- /sound/filename.mp3/file2.mp3
 * ・TWE-CTが2525と誤判定される問題の対応
 * 
 * 2019.10.8    1.2.2
 * ・mp3ファイル再生対応。 http:// -- /sound/filename.mp3/hogehoge...
 * 
 * 2019.10.8    1.2.1
 * ・初めてインストールするPCにて、ポート初期選択がNULLになることに起因するエラーの対策
 * ・日付変更によるバックアップ処理を+5分の遅延実行に変更
 * 
 * 2019.10.8    1.2.0
 * ・CSVフォーマット変更
 * ・バックアップ処理の追加
 * ・PAL(環境)、TWE2525(面)対応
 * ・-Nd、today指定対応
 * ・noheader対応
 * 
 * */
