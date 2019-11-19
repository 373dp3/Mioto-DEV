using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// アセンブリに関する一般情報は以下の属性セットをとおして制御されます。
// アセンブリに関連付けられている情報を変更するには、
// これらの属性値を変更してください。
[assembly: AssemblyTitle("MiotoServer")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("HP Inc.")]
[assembly: AssemblyProduct("MiotoServer")]
[assembly: AssemblyCopyright("Copyright © HP Inc. 2019")]
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
[assembly: AssemblyVersion("1.2.3")]
[assembly: AssemblyFileVersion("1.2.3")]

/*
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
