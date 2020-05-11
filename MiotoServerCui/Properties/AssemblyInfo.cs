using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// アセンブリに関する一般情報は以下の属性セットをとおして制御されます。
// アセンブリに関連付けられている情報を変更するには、
// これらの属性値を変更してください。
[assembly: AssemblyTitle("MiotoServer")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("dp3")]
[assembly: AssemblyProduct("MiotoServer")]
[assembly: AssemblyCopyright("Copyright © Toshiaki MINAMI 2018-2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// ComVisible を false に設定すると、その型はこのアセンブリ内で COM コンポーネントから 
// 参照不可能になります。COM からこのアセンブリ内の型にアクセスする場合は、
// その型の ComVisible 属性を true に設定してください。
[assembly: ComVisible(false)]

// このプロジェクトが COM に公開される場合、次の GUID が typelib の ID になります
[assembly: Guid("5da7a270-774c-4755-9638-12c33f5e641d")]

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
[assembly: AssemblyVersion("1.0.0.5")]
[assembly: AssemblyFileVersion("1.0.0.5")]

/**
 * 2020.5.11
 * ・停止・再開時にfinal、tNNNs指定ができない問題の対応
 * ・電流センサ向け「1」から始まるMACアドレス対応
 * ・mem:CT10へfinal、macフィルタ対応
 * 
 * 2019.12.4   1.0.4
 * ・yyyy化の漏れ対応
 * 
 * 2019.11.26   1.0.3
 * ・seqをdiff化
 * 
 * 2019.11.19   1.0.2
 * ・プレス機向けSingle Edge子機対応(btn 00のみ)
 * 
 * 2019.2.14 1.0.0.1
 * ・あんどん動作のため秒単位の情報が必要になったためCSV情報を拡張
 * ・AssemblyTitle、AssemblyProductをIoTBasicsからMiotoSeverに更新
 * 
 * */
