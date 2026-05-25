#include <IRremote.h>

#define IR_CAR_SPEED          80
#define MOTOR_DIRECTION       0

#define PIN_DIRECTION_LEFT    4
#define PIN_DIRECTION_RIGHT   3
#define PIN_MOTOR_PWM_LEFT    6
#define PIN_MOTOR_PWM_RIGHT   5
#define PIN_IRREMOTE_RECV     9

IRrecv irrecv(PIN_IRREMOTE_RECV);
decode_results results;

u32 lastKeyCode = 0;
u32 lastIRUpdateTime = 0;

void setup() {
  pinsSetup();
  irrecv.enableIRIn();
  
  Serial.begin(9600);
  Serial.println("=== Car Ready - Speed: " + String(IR_CAR_SPEED) + " ===");
  Serial.println("No buzzer alarm - Ready");
}

void loop() {
  if (irrecv.decode(&results)) {
    if (results.value != 0xFFFFFFFF) {
      lastKeyCode = results.value;
    }

    // Serial.print("IR Code: 0x");   // Comment out if you don't want logs
    // Serial.println(results.value, HEX);

    switch (lastKeyCode) {
      case 0xFF02FD: motorRun(IR_CAR_SPEED, IR_CAR_SPEED); break;   // Up
      case 0xFF9867: motorRun(-IR_CAR_SPEED, -IR_CAR_SPEED); break; // Down
      case 0xFFE01F: motorRun(-IR_CAR_SPEED, IR_CAR_SPEED); break;  // Left
      case 0xFF906F: motorRun(IR_CAR_SPEED, -IR_CAR_SPEED); break;  // Right
      case 0xFFA857: /* Center - no buzzer */ break;
    }
    
    irrecv.resume();
    lastIRUpdateTime = millis();
  } 
  else if (millis() - lastIRUpdateTime > 150) {
    motorRun(0, 0);
    lastIRUpdateTime = millis();
  }
}

void pinsSetup() {
  pinMode(PIN_DIRECTION_LEFT, OUTPUT);
  pinMode(PIN_DIRECTION_RIGHT, OUTPUT);
  pinMode(PIN_MOTOR_PWM_LEFT, OUTPUT);
  pinMode(PIN_MOTOR_PWM_RIGHT, OUTPUT);
  pinMode(A0, OUTPUT);
  digitalWrite(A0, LOW);     // Turn buzzer OFF
}

void motorRun(int speedl, int speedr) {
  int dirL = (speedl > 0) ? (0 ^ MOTOR_DIRECTION) : (1 ^ MOTOR_DIRECTION);
  if (speedl < 0) speedl = -speedl;

  int dirR = (speedr > 0) ? (1 ^ MOTOR_DIRECTION) : (0 ^ MOTOR_DIRECTION);
  if (speedr < 0) speedr = -speedr;

  speedl = constrain(speedl, 0, 255);
  speedr = constrain(speedr, 0, 255);

  digitalWrite(PIN_DIRECTION_LEFT, dirL);
  digitalWrite(PIN_DIRECTION_RIGHT, dirR);
  analogWrite(PIN_MOTOR_PWM_LEFT, speedl);
  analogWrite(PIN_MOTOR_PWM_RIGHT, speedr);
}