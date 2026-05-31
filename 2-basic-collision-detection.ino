#include <IRremote.hpp>

#define IR_CAR_SPEED      80
#define TURN_SPEED        120
#define MOTOR_DIRECTION   0

#define PIN_DIRECTION_LEFT    4
#define PIN_DIRECTION_RIGHT   3
#define PIN_MOTOR_PWM_RIGHT   5
#define PIN_MOTOR_PWM_LEFT    6
#define TRIGGER_PIN           7
#define ECHO_PIN              8
#define PIN_IRREMOTE_RECV     9

const int STOPPING_DISTANCE = 30;

IRrecv irrecv(PIN_IRREMOTE_RECV);

unsigned long lastKeyCode = 0;
unsigned long lastIRUpdateTime = 0;

int distance = 0;
bool isMovingForward = false;

void setup() {
  pinsSetup();
  IrReceiver.begin(PIN_IRREMOTE_RECV, ENABLE_LED_FEEDBACK);
  
  Serial.begin(9600);
  Serial.println("=== Car Ready ===");
}

void loop() {
  // IR Remote Control
  if (IrReceiver.decode()) {
    if (IrReceiver.decodedIRData.decodedRawData != 0) {
      lastKeyCode = IrReceiver.decodedIRData.decodedRawData;
    }

    switch (lastKeyCode) {
      case 0xBF40FF00: 
        isMovingForward = true;
        motorRun(IR_CAR_SPEED, IR_CAR_SPEED); 
        break;
      case 0xE619FF00: 
        isMovingForward = false;
        motorRun(-IR_CAR_SPEED, -IR_CAR_SPEED); 
        break;
      case 0xF807FF00: 
        isMovingForward = false;
        motorRun(-TURN_SPEED, TURN_SPEED); 
        break;
      case 0xF609FF00: 
        isMovingForward = false;
        motorRun(TURN_SPEED, -TURN_SPEED); 
        break;
      case 0xFFA857:   
        isMovingForward = false;
        motorRun(0, 0); 
        break;
    }
    
    IrReceiver.resume();
    lastIRUpdateTime = millis();
  } 
  else if (millis() - lastIRUpdateTime > 120) {
    motorRun(0, 0);
    isMovingForward = false;
    lastIRUpdateTime = millis();
  }

  // Read Ultrasonic Sensor
  digitalWrite(TRIGGER_PIN, LOW);
  delayMicroseconds(2);
  digitalWrite(TRIGGER_PIN, HIGH);
  delayMicroseconds(10);
  digitalWrite(TRIGGER_PIN, LOW);
  
  long duration = pulseIn(ECHO_PIN, HIGH, 50000);
  distance = duration * 0.030 / 2;

  Serial.print("Distance: ");
  Serial.print(distance);
  Serial.println(" cm");

  // Only stop if going forward
  if (isMovingForward && distance > 0 && distance < STOPPING_DISTANCE) {
    Serial.println("! Too close! Stopping the vehicle.");
    motorRun(0, 0);
    isMovingForward = false;
  }

  delay(40);
}

// HELPER FUNCTIONS
void pinsSetup() {
  pinMode(PIN_DIRECTION_LEFT, OUTPUT);
  pinMode(PIN_DIRECTION_RIGHT, OUTPUT);
  pinMode(PIN_MOTOR_PWM_LEFT, OUTPUT);
  pinMode(PIN_MOTOR_PWM_RIGHT, OUTPUT);
  pinMode(TRIGGER_PIN, OUTPUT);
  pinMode(ECHO_PIN, INPUT);
  pinMode(A0, OUTPUT);
  digitalWrite(A0, LOW);
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
