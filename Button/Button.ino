/*
Name:		Button.ino
Created:	2018/09/15 20:17:56
Author:	Toshiaki MINAMI

Copyright (c) 2018 Toshiaki Minami
Released under the MIT license
https://opensource.org/licenses/mit-license.php

*/

#define PIN_PRG_BTN		0
static volatile int btnCnt = 0;
static volatile bool isHit = false;

unsigned long photoOnMs = 0;
unsigned long photoOffMs = 0;
unsigned long photoDeltaMs = 0;

void onChange() {
	if (digitalRead(PIN_PRG_BTN) == LOW) {
		//点灯している
		photoOnMs = millis();
		isHit = true;
	}
	else {
		//消灯している
		photoOffMs = millis();
		if (photoOnMs > 0) {
			photoDeltaMs = photoOffMs - photoOnMs;
		}
	}
}

void setup() {
	Serial.begin(38400);
	delay(1000);

	pinMode(PIN_PRG_BTN, INPUT);
	pinMode(16, OUTPUT);
	pinMode(14, OUTPUT);
	digitalWrite(16, LOW);
	digitalWrite(14, HIGH);

	//FALLING:H->L時、RISING:L->H時、CHANGE:状態変更時
	attachInterrupt(PIN_PRG_BTN, onChange, CHANGE);
}

void loop() {
	static unsigned long ledOnLimitMs = millis()+10000;
	static unsigned long ledOffLimitMs = 0;
	static unsigned long beepOnMs = 0;
	static unsigned long beepOffMs = 0;

	if ((millis() > ledOnLimitMs) && (ledOnLimitMs!=0) ) {
		ledOffLimitMs = millis() + 5000;
		ledOnLimitMs = 0;
		digitalWrite(14, LOW);
	}
	if ((millis() > ledOffLimitMs) && (ledOffLimitMs!=0)) {
		ledOnLimitMs = millis() + 10000;
		ledOffLimitMs = 0;
		digitalWrite(14, HIGH);
	}

	//点灯状態か？
	if (isHit==true) {
		//２秒前にビープ
		beepOnMs = millis() + photoDeltaMs - 2000;
		beepOffMs = beepOnMs + 1000;
		Serial.println("Update");
		isHit = false;

	}

	if ((millis()>beepOnMs) && (millis()<beepOffMs)) {
		digitalWrite(16, HIGH);
		Serial.print("ON\t");
	}
	else {
		digitalWrite(16, LOW);
		Serial.print("OFF\t");
	}
	Serial.printf("%ld,%ld,%ld,%ld\n", millis(), beepOnMs, beepOffMs, photoOnMs);
	delay(200);
}

//以下、falseは本来のButton.ino
#if false
/*
Name:		Button.ino
Created:	2018/09/15 20:17:56
Author:	Toshiaki MINAMI

Copyright (c) 2018 Toshiaki Minami
Released under the MIT license
https://opensource.org/licenses/mit-license.php

*/

#define PIN_PRG_BTN		0
static volatile int btnCnt = 0;
bool isHit = false;

void onFalling() {
	if (isHit) { return; }
	btnCnt++;
	isHit = true;
}

void setup() {
	Serial.begin(38400);
	delay(1000);

	pinMode(PIN_PRG_BTN, INPUT);

	//FALLING:H->L時、RISING:L->H時、CHANGE:状態変更時
	attachInterrupt(PIN_PRG_BTN, onFalling, FALLING);
}

void loop() {
	if (digitalRead(PIN_PRG_BTN) == HIGH) {
		Serial.printf("High %d\n", btnCnt);
	}
	else {
		Serial.printf("Low %d\n", btnCnt);
	}
	isHit = false;
	delay(1000);
}
#endif
