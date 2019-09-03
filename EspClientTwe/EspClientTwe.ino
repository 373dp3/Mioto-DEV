/*
Name:		EspClient.ino
Created:	2018/09/16 10:29:09
Author:	Toshiaki MINAMI

Copyright (c) 2018 Toshiaki Minami
Released under the MIT license
https://opensource.org/licenses/mit-license.php

*/
#include <ESP8266WiFi.h>
#include <ESP8266HTTPClient.h>


#define PIN_PRG_BTN		0
uint8 btnCnt = 0;
bool isPost = false;

//[TODO]ssid, passwd, post_urlはお使いのWiFi環境にあわせて修正してください
char ssid[] = "W04_F0C850AF2755";
char passwd[] = "atqbjbmjba69g65";
char post_url[] = "http://192.168.100.169/post/";

//入力対象のピン一覧(16はリセット用として使用する)
uint8 pinAry[] = { 0, 2, 4, 5, 12, 13, 14, 15 };

#define RETRY_COUNT		1

void onChange() {
	btnCnt++;
	isPost = true;
}

uint8 getBtnFlg() {
	uint8 btn = 0;
	for (int i = sizeof(pinAry) - 1; i > 0; i--) {
		btn += digitalRead(pinAry[i]) ? 0 : 1;
		btn = btn << 1;
	}
	btn += digitalRead(pinAry[0]) ? 0 : 1;
	return btn;
}

void post() {
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

	//TWE-Liteのパケットを模してメッセージを構築
	sprintf(postBuffer, 
		":7FD30101%02X%02X%02X%02X%02X00000000FFFF%02X%02X00000000000000",
		(uint8)lqi, 
		mac0[2], mac0[3], mac0[4], mac0[5],
		btnCnt, 
		getBtnFlg()/*Btn*/
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
	Serial.printf("JSON POST: %s\n", postBuffer);

	//http post実処理
	for(int i=0; i<RETRY_COUNT + 1; i++) {
		unsigned long stms = millis();
		int httpCode = http.POST(postBuffer);
		if (httpCode > 0) {
			Serial.printf("time ms %d\n", millis() - stms);
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

	//FALLING:H->L時、RISING:L->H時、CHANGE:状態変更時
	for (int i = 0 ; i <sizeof(pinAry); i++) {
		pinMode(pinAry[i], INPUT_PULLUP);
		attachInterrupt(pinAry[i], onChange, CHANGE);
	}

	WiFi.mode(WIFI_STA);
	WiFi.begin(ssid, passwd);

	Serial.print(F("\nWiFi connecting"));
	while (WiFi.status() != WL_CONNECTED) {
		Serial.print("."); delay(1000);
	}

	Serial.printf("\nWifi connected. ip: %s\n", WiFi.localIP().toString().c_str());

}

void loop() {
	if (isPost) { post(); }
}
