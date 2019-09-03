var graphBatt = null;
var graphCtAryObjAry = null;
var graphDate = null;
var graphTemp = null;
var settingJson = null;
var page_width = -1;
var refCnt = 0;
var timer = false;
var isI2cTempEnable = false;
var rssi5 = -1;
$(window).resize(function() {
    if (timer !== false) {
        clearTimeout(timer);
    }
    timer = setTimeout(function() {
    	if(page_width != window.innerWidth){
			refresh();
			refCnt++;
			$("#debug").html("cnt:"+refCnt);

    	}
    	page_width = window.innerWidth;
    }, 100);
});

window.onload = function () {
	//コンテンツを非表示
	//$('#mainContents').hide();
	var ua, isIE, array, version;
	ua = window.navigator.userAgent.toLowerCase();
	isIE = (ua.indexOf('msie') >= 0 || ua.indexOf('trident') >= 0);
	 
	// IE の場合、バージョンを取得
	if (isIE) {
		$("#subinfo").html("<p>お使いのブラウザはサポート対象外です。"
		+"サイトをご覧頂くにはGoogle Chromeなどのモダンブラウザをお使い下さい。</p>"
		+"<p><a href=\"https://www.google.co.jp/chrome/index.html\">パソコン版Chrome</a></p>"
		+"<p><a href=\"https://www.google.co.jp/chrome/browser/mobile/index.html\">モバイル版Chrome</a></p>");
	}else{
		//$("#subinfo").html("<p>"+ ua +"</p>");
	}
	
	var res = $('#mainContents');
	if(res == null) { console.log("Res is null"); }

	//Char APIの事前ロード
	google.charts.load('current', {'packages':['corechart']});
	page_width = window.innerWidth;
	refresh();
}



function refresh(){
	var param = getUrlParam();
	var reg = /[\d]{4}\/[\d]{4}/;
	var isYmdHit = 0;
	if(param.ymd != undefined)
	{
		param.ymd = decodeURIComponent(param.ymd);
		if(reg.test(param.ymd)){
			isYmdHit=1;
		}
	}
	var csvpath = "";
	if(isYmdHit)
	{
		csvpath = "csv/"+param.id + "/" + param.ymd + ".csv";
	}else{
		csvpath = "csv/"+param.id + "/" + getYmdPath(0);
	}
	var configpath = "csv/"+param.id + "/setting.json";
	getAllData(configpath, csvpath);
	
}

function getAllData(configpath, csvpath){

	if(configpath.indexOf("http")>=0) { console.error("不適切なURL指定"); return; }
	if(csvpath.indexOf("http")>=0) { console.error("不適切なURL指定"); return; }
	setDateLink();

	$.ajax({
		url: configpath,
		type: "GET",
		cache: false,
		dataType: "json",
		success: function(msg){
			settingJson = msg;
			getCsv(csvpath);
			$("#plantName")[0].innerText = settingJson["plantName"];
			$("#titleName")[0].innerText = settingJson["plantName"] + " :: Solar ReMon";
		},
		error: function(msg) {
			settingJson = new Object();
			getCsv(csvpath);

		}
	});
}



//Google Char API callback
function drawChart(){
	var dtStart = new Date(graphDate);
	dtStart.setHours(4);
	dtStart.setMinutes(0);
	dtStart.setMilliseconds(0);
	var dtEnd = new Date(graphDate);
	dtEnd.setHours(20);
	dtEnd.setMinutes(0);
	dtEnd.setMilliseconds(0);

	if(graphBatt != null)
	{
		var data = google.visualization.arrayToDataTable(graphBatt);
		var options = {
				title: '鉛電池電圧(6.1v未満の場合、電池保護のため情報は送信されません)',
				curveType: 'none',
				vAxis: {maxValue: 6.4, minValue: 6},
				hAxis: {maxValue: dtEnd, minValue: dtStart},
				legend: { position: 'bottom' }
	        };

		var chart = new google.visualization.LineChart(document.getElementById('curve_chart_batt'));
		chart.draw(data, options);
	}else
	{
		$("#curve_chart_batt").hide();
	}

	if(graphTemp != null)
	{
		var data = google.visualization.arrayToDataTable(graphTemp);
		var options = {
				title: '温度(℃)',
				curveType: 'none',
				vAxis: { minValue: 0},
				hAxis: {maxValue: dtEnd, minValue: dtStart},
				legend: { position: 'bottom' }
	        };

		var chart = new google.visualization.LineChart(document.getElementById('curve_chart_temp'));
		chart.draw(data, options);
	}else
	{
		$("#curve_chart_temp").hide();
	}


	//infoTypeを昇順でソート
	var keys = Object.keys(graphCtAryObjAry).sort(function(a,b){ return a - b ;});
	
	//グラフエリアの暫定非表示
	for(var i=0; i<3; i++) { $("#curve_chart_ct_"+i).hide(); }

	//描画
	keys.forEach(function(key){
		var obj = graphCtAryObjAry[key];
		var dat = google.visualization.arrayToDataTable(obj.ary);
		var opt = {
			title: obj.title,
			curveType: 'none',
			vAxis: {minValue: 0},
			hAxis: {maxValue: dtEnd, minValue: dtStart},
			legend: { position: 'bottom' }
		};
		$("#curve_chart_ct_"+key).show();//データのあるもののみを表示
		var chart = new google.visualization.LineChart(document.getElementById('curve_chart_ct_'+key));
		chart.draw(dat, opt);

	});
}

function setDateLink(){
	var param = getUrlParam();
	var uri = "?t="+param.t
			+ "&id="+param.id 
			+ "&b="+param.b;

	var baseDate = new Date();
	if(param.ymd != undefined){
		var ymdAry = decodeURIComponent(param.ymd).split("/");
		var ymdMon = Math.floor(Number(ymdAry[1])/100);
		var ymdDay = Number(ymdAry[1]) - ymdMon*100;
		baseDate = new Date(ymdAry[0], ymdMon - 1, ymdDay);
		var dt = new Date();
		var diff = Math.floor((baseDate.getTime() 
				- (new Date(dt.getFullYear(), dt.getMonth(), dt.getDate())).getTime())/(24*3600*1000));
		param["ofs"] = diff;
	}else if(param.ofs != undefined){
		baseDate.setDate(baseDate.getDate() + Number(param.ofs));
	}
	var tmpDay = new Date();
	
	var daylink = '<a href="' + uri //+  '&ofs=' + (param.ofs - 30) 
					+ '&ymd=' + encodeURIComponent(getYmdByBaseDateOffset(baseDate, -30)) +'">-30日</a>　';
	daylink += '<a href="' + uri //+'&ofs=' + (param.ofs - 7)
				+ '&ymd=' + encodeURIComponent(getYmdByBaseDateOffset(baseDate, -7))+'">-7日</a>　';
	daylink += '<a href="' + uri //+ '&ofs=' + (param.ofs - 1)
				+ '&ymd=' + encodeURIComponent(getYmdByBaseDateOffset(baseDate, -1))
				+'">-1日</a>　|　';
	if(param.ofs < 0){
		daylink += '<a href="' + uri //+ '&ofs=' + (param.ofs + 1)
					+ '&ymd=' + encodeURIComponent(getYmdByBaseDateOffset(baseDate, 1))
					+'">+1日</a>　';
	}else{
		daylink += '+1日　';
	}
	if(param.ofs < -6){
		daylink += '<a href="' + uri //+ '&ofs=' + (param.ofs + 7)
					+ '&ymd=' + encodeURIComponent(getYmdByBaseDateOffset(baseDate, 7))
					+'">+7日</a>　';
	}else{
		daylink += '+7日　';
	}
	if(param.ofs < -29){
		daylink += '<a href="' + uri //+ '&ofs=' + (param.ofs + 30)
					+ '&ymd=' + encodeURIComponent(getYmdByBaseDateOffset(baseDate, 30))
					+'">+30日</a>　';
	}else{
		daylink += '+30日　';
	}
	if(param.ofs==0){
		daylink += '|　今日';
	}else{
		daylink += '|　<a href="' + uri + '&ofs=' + (0)+'">今日</a>';
	}

	$("#daylink").html(daylink);

}

//バッテリグラフ用のGoogle Chartタグの生成
function prepareGraph(csvObj){

	var param = getUrlParam();

	//サブタイトルの更新
	var obj1st = csvObj.ary[0];
	var objLast = csvObj.ary[csvObj.ary.length-1];
	var subinfo = obj1st.dt.toLocaleDateString() 
			+ " " + obj1st.dt.toLocaleTimeString() + " - "
			+ " " + objLast.dt.toLocaleTimeString();
	if(rssi5>=0) {
		subinfo += "　電波強度: " + rssi5 + " / 5";
	}
	
	//サブタイトル 編集リンクへ
	var uri = "?t="+param.t
			+ "&id="+param.id 
			+ "&b="+param.b
			+ "&_ss="+(new Date()).getTime();
	{
		subinfo += "　[<a href=\"edit.html"+ uri +"\">";
		subinfo += "設定変更</a>] ";
	}
	
	$("#subinfo").html("<p>"+subinfo+"</p>");
		
	graphDate = obj1st.dt;

	//バッテリ電圧描画用変数にセット
	if(param.b == 1)
	{
		graphBatt = new Array();
		//凡例
		graphBatt.push(['日時','電圧']);
		//データ
		csvObj.ary.forEach(function(data){
			if(data==undefined) { return; }
			if(data.ctObj.infoType != 0) { return; }
			var ary = new Array();
			ary.push(data.dt);
			ary.push(data.batt);
			graphBatt.push(ary);
		});
	}
	
	//温度センサの値
	if(param.t > 0)
	{
		graphTemp = new Array();
		var infoType = Math.floor(param.t / 10);
		var chNo = param.t - infoType * 10;
		//凡例
		graphTemp.push(['日時','温度']);
		csvObj.graphValidPosObj[infoType]--;//温度は末尾に存在。無効化
		//データ
		csvObj.ary.forEach(function(data){
			if(data==undefined) { return; }
			if(data.ctObj.infoType != infoType) { return; }
			var ary = new Array();
			ary.push(data.dt);
			ary.push(Math.floor(data.ctObj[infoType][chNo] * 1100/256+0.5)/10);
			graphTemp.push(ary);
		});
	}
	//I2Cタイプの温度センサ(0,7)
	if(isI2cTempEnable && (graphTemp == null)) {
		graphTemp = new Array();
		var infoType = 0;
		//凡例
		graphTemp.push(['日時','温度']);
		csvObj.graphValidPosObj[infoType]--;//温度は末尾に存在。無効化
		//データ
		csvObj.ary.forEach(function(data){
			if(data==undefined) { return; }
			if(data.ctObj.infoType != infoType) { return; }
			var ary = new Array();
			ary.push(data.dt);
			ary.push(0.5*(data.ctObj[infoType][data.ctObj[infoType].length - 1] - 80));
			graphTemp.push(ary);
		});
	}

	//同様に、電流センサの値も処理
	graphCtAryObjAry = new Array();
	csvObj.infoTypeAry.forEach(function(key){
		var obj = new Object();
		//obj.title = "電流センサモジュール "+(key + 1) + " 電流指標値";
		if(settingJson["inputCurrentUnit"+key+"Label"] != undefined){
			obj.title = "ユニット"+(key + 1) + " "+settingJson["inputCurrentUnit"+key+"Label"]
				+" 電流(A)";
		}else{
			obj.title = "電流センサユニット"+(key + 1) + " 電流(A)";
		}
		obj.ary = new Array();
		var pos = csvObj.graphValidPosObj[key];//有効な配列数
		
		var legAry = new Array();
		legAry.push("日時");
		for(var i=0; i<pos; i++){
			if(settingJson["chkCt"+key+"Ch"+i] != "on") { continue; }
			legAry.push("ch:" + (i+1));
		}
		obj.ary.push(legAry);
		
		//描画用配列に値を格納
		csvObj.ary.forEach(function(data){
			if(data.ctObj.infoType != key) { return; }
			var ary = new Array();
			ary.push(data.dt);
			for(var i=0; i<pos; i++){
				if(settingJson["chkCt"+key+"Ch"+i] != "on") { continue; }
				var val = data.ctObj[key][i];
				//無効値を0に
				if((val == 255) || (val == undefined)) { val = 0; }
				//ディジット->電流値変換
				val *= 0.484609962;//負荷抵抗10Ω,2000:1,半波整流時の係数
				ary.push(val);
			}
			obj.ary.push(ary);//1行分の実データ(日付、数値1,2,3...)
		});
		graphCtAryObjAry.push(obj);//infoType毎のグラブ描画元情報
	});

	//Google Chart API呼び出し
	google.charts.setOnLoadCallback(drawChart);

	
}

//CSV１行の解釈
function parseCsvLine(line, param) {
	var node = line.split(",");
	if(node.length < 2){ return; }
	var obj = new Object();
	obj.dt = new Date(node[0]);
	obj.ctObj = new Object();

	//電流センサ値の格納場所を必要であれば確保
	var infoType = node[2];
	if(infoType == undefined) { infoType = 0; }
	if(obj.ctObj[infoType] == undefined) {
		obj.ctObj[infoType] = new Array();
		obj.ctObj.infoType = Number(infoType);
	}
	
	var id = Number(node[2]);
	
	if(node[1].includes("00000000000000")) { return null; }
	if(node[1]=="1") { return null; /* スイッチ警告回避 */}
	
	for(var i=0; i<16; i+=2){
		var val = parseInt("0x"+(node[1].substring(i, i+2)), 16);
		switch (i){
			case 0://アンテナ
				obj.ant = val;
				if(val != 0) {//AC給電にてエラーで0になる事があるため。
					rssi5 = val;
				}
				break;
			case 2://バッテリ電圧
				if((id==0) && (param.b==1)){
					obj.batt = val;
					break;
				}
				obj.batt = 0;
				//それ以外の場合、デフォルトの処理へ
			case 14:
				if((val!=0) && (val!=0xff) && (param.t == "")){
					isI2cTempEnable = true;
				}
			default:
				obj.ctObj[infoType].push(val);
		}
	}
	//破損データチェック
	if(infoType == "1"){
		for(var i=0; i<obj.ctObj[infoType].length - 1; i++){
			var val = obj.ctObj[infoType][i];
			if(val == 255) { return null;}
		}
	}
	if(node.length==3) {//初期試作は2
		obj.batt = (obj.batt+500)/100;
		if(obj.batt<6) { //7.56以上。8bit溢れ
			obj.batt = obj.batt + 2.46;
		}
	}else{
		obj.batt = obj.batt / 256 * 1.1 * 11;
	}
	
	return obj;
}

//CSVファイルの解釈
function parseCsv(path, msg) {
	var obj = new Object();
	var param = getUrlParam();
	obj.path = path;
	obj.ary = new Array();
	obj.infoTypeAry = new Array();//type0, 1, .. 毎の処理を効率化するための事前準備
	obj.graphValidPosObj = new Object();//格納情報の有効位置(0xFF排除用)
	msg.split(/\r\n|\r|\n/).forEach(function(line){
		var ans = parseCsvLine(line, param);
		if(ans == null) { return; }
		obj.ary.push(ans);
		
		//typeの一覧を整理
		if(obj.infoTypeAry.includes(ans.ctObj.infoType)==false){
			obj.infoTypeAry.push(ans.ctObj.infoType);
		}
		var infotype = ans.ctObj.infoType;
		for(var i=0; i<ans.ctObj[infotype].length; i++){
			if((ans.ctObj[infotype][i] >= 255) && (//0xFFが無効値
				(obj.graphValidPosObj[infotype]==null)
				|| (i < obj.graphValidPosObj[infotype])
				)){
				obj.graphValidPosObj[infotype] = i;
				break;
			}
		}
		if(obj.graphValidPosObj[infotype] == null){
			obj.graphValidPosObj[infotype] = ans.ctObj[infotype].length;
		}
	});

	//低温補正	
	if(true){
		var threshold = 5*60*1000;//ms
		var origineI = -1;
		var origineDate = null;
		for(var i=1; i<obj.ary.length; i++){
			var delta = obj.ary[i].dt.getTime() - obj.ary[i-1].dt.getTime();
			if(delta < threshold * 1.1){
				console.log(delta);
				origineI = i-1;
				origineDate = obj.ary[i-1].dt;
				break;
			}
		}
		for(var i=0; i<origineI; i++){
			obj.ary[i].dt = new Date(origineDate.getTime() - (origineI-i)*28*60*1000);
		}
	}

	obj.infoTypeAry.sort();
	return obj;
}

//CSVファイルの取得
function getCsv(path){
	if(path.indexOf("http")>=0) { console.error("不適切なURL指定"); return; }
	setDateLink();

	$.ajax({
		url: path,
		type: "GET",
		cache: false,
		dataType: "text",
		success: function(msg){ 
			var obj = parseCsv(path, msg);
			prepareGraph(obj);
			if(loadAdminForm != undefined){
				loadAdminForm(obj);
			}
		},
		error: function(msg) {
			console.error("error :" + msg);
			var dt = new Date();
			dt.setDate(dt.getDate() + getUrlParam().ofs);
			var dtString = "今日";
			if(getUrlParam().ofs!=0){
				dtString = dt.toLocaleDateString();
			}


			$("#subinfo").html("<p class=\"text-danger\">" + dtString + "の情報はありません</p>");

		}
	});
}

//日付からPATHの生成
function getYmdPath(offsetDays){
	return getYmdByOffset(offsetDays)+".csv";
}
function getYmdByOffset(offsetDays){
	var dt = new Date();
	dt.setDate(dt.getDate() + offsetDays);
	var path = dt.getFullYear() + "/"
			 + (("0") + (dt.getMonth()+1)).substr(-2)
			 + (("0") + (dt.getDate())).substr(-2);
	return path;
}
function getYmdByBaseDateOffset(baseDate, offsetDays){
	var dt = new Date(baseDate);
	dt.setDate(baseDate.getDate() + offsetDays);
	var path = dt.getFullYear() + "/"
			 + (("0") + (dt.getMonth()+1)).substr(-2)
			 + (("0") + (dt.getDate())).substr(-2);
	return path;
}

//引数からパラメタの取得
function getUrlParam(){
	var url = location.href.split("?");

	if(url.length != 2){ console.error("url parameter error"); }
	
	var ans = new Object();	

	var param = url[1].split("&");
	param.forEach(function(valueKey){
		var vk = valueKey.split("=");
		if(vk.length==2){ ans[vk[0]] = vk[1]; }
	});
	
	//引数が省略された場合の初期パラメタ
	if(ans.ofs == undefined) { ans.ofs = 0; }
	//if(ans.t == undefined || (ans.t == "")) { ans.t = 0; }//温度
	if(ans.b == undefined || (ans.b == "")) { ans.b = 1; }//バッテリ 1:有効, 0:無効。初期値有効
	ans.ofs = Number(ans.ofs);

	return ans;
}




