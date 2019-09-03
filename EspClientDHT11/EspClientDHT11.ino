/*
Name:		EspClientTwe.ino
Created:	2019/07/09 19:12:19
Author:	Toshiaki MINAMI
*/

extern "C" {
#include "user_interface.h"
}

#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>
#include <DHT.h>

const int PIN_DHT = 12;
DHT dht(PIN_DHT, DHT11);


#define PIN_PRG_BTN		0
uint8 btnCnt = 0;
bool isPost = false;


//[TODO]ssid, passwd, post_urlはお使いのWiFi環境にあわせて修正してください
char ssid[] = "SPWN_N35_3597c0";
char passwd[] = "1c64c5556f1d7";
char post_url[] = "http://192.168.179.5/post/";


#define RETRY_COUNT		1



void post() {
	float humidity = dht.readHumidity();
	float temperature = dht.readTemperature();
	if (temperature < 0) {
		temperature = 0;
	}
	if (humidity < 0) {
		humidity = 0;
	}

	HTTPClient http;
	http.begin(post_url);

	//MACアドレスの取得
	uint8_t mac0[6];
	WiFi.macAddress(mac0);

	//LQI変換は https://mono-wireless.com/jp/tech/Programming/Tips_LQI.html を参考に
	long lqi = (1970 + 20 * WiFi.RSSI()) / 7;
	if (lqi < 0) { lqi = 0; }
	if (lqi > 255) { lqi = 255; }

	char* postBuffer = (char*)malloc(55);
	memset(postBuffer, 0, 50);
	if (postBuffer == NULL) { Serial.println(F("[ERROR] malloc failed")); }

	uint16 temp = (temperature * 10);
	Serial.printf("temp: %d\n", temp);
	uint8 u8MSB_temp = (uint8)(temp >> 2);
	uint8 u8LSB = (uint8)(0x0003 & temp);//下位2ビット抽出

	uint16 humi = (humidity * 10);
	Serial.printf("humi: %d\n", humi);
	uint8 u8MSB_humi = (uint8)(humi >> 2);
	u8LSB |= (0x000C & (humi << 2));


	//TWE-Liteのパケットを模してメッセージを構築
	//7F81の81はTWE-Lite純正の初期ファームウェアにおけるApplication ID
	sprintf(postBuffer,
		":7F810101%02X%02X%02X%02X%02X00000000FFFF000000%02X%02X0000%02X00",
		(uint8)lqi,
		mac0[2], mac0[3], mac0[4], mac0[5],
		u8MSB_temp,//AD上位8ビット
		u8MSB_humi,//AD上位8ビット
		u8LSB//AD下位2ビット
	);

	//LRC計算
	uint8 lrc = 0;
	char datum[3] = { 0, 0, 0 };
	for (int i = 1; i < 46; i += 2) {
		memcpy(datum, &(postBuffer[i]), 2);
		lrc += (uint8)strtol(datum, NULL, 16);
		//Serial.printf("i:%d, org:%s, val: %d\n", i, datum, lrc);
	}
	lrc = ~lrc + 1;//2の補数
	sprintf(&(postBuffer[47]), "%02X", lrc);
	Serial.printf("POST: %s\n", postBuffer);

	//http post実処理
	for (int i = 0; i < RETRY_COUNT + 1; i++) {
		int httpCode = http.POST(postBuffer);
		if (httpCode > 0) {
			Serial.printf("httpCode: %d\n", httpCode);
			Serial.printf("payload: %s\n", http.getString().c_str());
			http.end();
			break;
		}
		else {
			Serial.println(F("Error on HTTP request"));
			http.end();
			delay(3000);
		}
	}

	//メモリ解放
	free(postBuffer);
	isPost = false;
}

void setup() {
	Serial.begin(38400);

	WiFi.mode(WIFI_STA);
	WiFi.begin(ssid, passwd);

	Serial.print(F("\nWiFi connecting"));
	while (WiFi.status() != WL_CONNECTED) {
		Serial.print("."); delay(1000);
	}

	Serial.printf("\nWifi connected. ip: %s\n", WiFi.localIP().toString().c_str());
	dht.begin();
}

void loop() {
	post();
	delay(10 * 1000);//約10秒に1回のループ
}
