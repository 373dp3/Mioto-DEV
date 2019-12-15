using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// アセンブリに関する一般情報は以下の属性セットをとおして制御されます。
// アセンブリに関連付けられている情報を変更するには、
// これらの属性値を変更してください。
[assembly: AssemblyTitle("CalcHelper")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("HP Inc.")]
[assembly: AssemblyProduct("CalcHelper")]
[assembly: AssemblyCopyright("Copyright © HP Inc. 2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// ComVisible を false に設定すると、その型はこのアセンブリ内で COM コンポーネントから 
// 参照不可能になります。COM からこのアセンブリ内の型にアクセスする場合は、
// その型の ComVisible 属性を true に設定してください。
[assembly: ComVisible(false)]

// このプロジェクトが COM に公開される場合、次の GUID が typelib の ID になります
[assembly: Guid("8be3da5e-0d72-4425-80b2-ba6b9ccf838f")]

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
[assembly: AssemblyVersion("1.0.2.0")]
[assembly: AssemblyFileVersion("1.0.2.0")]

/*
 * 2019.12.5    1.0.2.0
 * インストーラのTask登録3日制限問題対応
 * 
 * 2019.7.1     1.0.1.0
 * 
 *  WiFi切断等のネットワーク変更が生じた場合、それ以降のhttp接続が一切
 *  行われなくなる問題点の解消
 *  
 *  レスポンスヘッダにLast-Modifiedを追加
 *  
 *  リクエストヘッダにIf-Modified-sinceがある場合、304 Not modified返答に対応
 *  
 * */