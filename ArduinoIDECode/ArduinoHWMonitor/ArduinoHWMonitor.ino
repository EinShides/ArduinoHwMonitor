#include <UTFTGLUE.h>
#include  <string.h>
UTFTGLUE myGLCD(0,A2,A1,A3,A4,A0);

#include "Adafruit_GFX.h"
#include "MCUFRIEND_kbv.h"
MCUFRIEND_kbv tft(A3, A2, A1, A0, A4);
//Formatting: each character is 16 pixels wide

String inData;

//declares string with set amount of spaces
String CPUTemp = String(' ', 2);
String CPUFreq = String(' ', 4);
String GPUCoreTemp = String(' ', 2);
String GPUMemTemp = String(' ', 2);
String GPUHotSpotTemp = String(' ', 2);
String GPUCoreClock = String(' ', 4);
String GPUMemUsage = String(' ', 2);
String CPULoad = String(' ', 2);
String GPULoad = String(' ', 2);


//display row function
int line (int rowNum) {
  int result;
  result = rowNum * 20;
  return result;
}


//function to keep value the same length and only update when needed (used to reduce display flickering)
void displayInfo (String newValue, String oldDisplayValue, int maxLength, int xpos, int ypos) {
  
  String zeroes;
  String nines;
  String displayValue;
  
  if (newValue.length() > maxLength) {
    for (int i = 0; i < maxLength; i++) {
      nines += '9';
    }
    displayValue = nines;
  }

  else if (newValue.length() == maxLength) {
      displayValue = newValue;
  }

  else if (newValue.length() < maxLength) {
      for (int i = 0; i < maxLength - newValue.length(); i++) {
        zeroes += '0';
      }
      displayValue = zeroes + newValue;
  }
  
  if (displayValue != oldDisplayValue) {
    oldDisplayValue = displayValue;
    myGLCD.print(displayValue, xpos, ypos);
  }
}



void setup() {

  Serial.begin(115200);

  randomSeed(analogRead(0));

  myGLCD.InitLCD(3);
  myGLCD.setFont(BigFont);
  myGLCD.clrScr();

  //set black background
  //myGLCD.fillScr(255, 255, 255);

  //CPU stat labels
  myGLCD.setTextColor(0xf800);
  //myGLCD.settextColor(255, 0, 0);
  myGLCD.print("CPU", 10, line(1));

  myGLCD.print("CPU:", 300, line(1));

  myGLCD.setTextColor(0xffff);
  myGLCD.print("%", 360+16*2, line(1));

  myGLCD.print("Temp:", 10, line(2));
  myGLCD.print("C", 85+16*2, line(2));

  myGLCD.print("Freq:", 10, line(3));
  myGLCD.print("MHz", 85+16*4, line(3));

  //GPU stat labels
  myGLCD.setTextColor(0x07e0);
  myGLCD.print("GPU", 10, line(5));

  myGLCD.print("GPU:", 300, line(2));  

  myGLCD.setTextColor(0xffff);
  myGLCD.print("%", 360+16*2, line(2));

  myGLCD.print("Core Temp:", 10, line(6));
  myGLCD.print("C", 165+16*2, line(6));

  myGLCD.print("Mem Temp:", 10, line(7));
  myGLCD.print("C", 150+16*2, line(7));

  myGLCD.print("Hotspot Temp:", 10, line(8));
  myGLCD.print("C", 215+16*2, line(8));

  myGLCD.print("Core Clock:", 10, line(10));
  myGLCD.print("MHz", 180+16*4, line(10));

  myGLCD.print("VRAM Usage:", 10, line(11));
  myGLCD.print("%", 180+16*2, line(11));

  myGLCD.setColor(255, 255, 255);
}

void loop() {

  //Main loop to recieve hardware and display hardware info
  while(Serial.available() > 0){
    char recieved = Serial.read();
    inData += recieved;

    //CPU Temp
    if (recieved == 'a'){
      inData.remove(inData.length() - 1, 1);
      displayInfo(inData, CPUTemp, 2, 85, line(2));
      inData = "";

    }

    //CPU Freq
    if (recieved == 'b'){
      inData.remove(inData.length() - 1, 1);
      displayInfo(inData, CPUFreq, 4, 85, line(3));
      inData = "";
    }

    //GPU Core Temp
    if (recieved == 'c'){
      inData.remove(inData.length() - 1, 1);
      displayInfo(inData, GPUCoreTemp, 2, 165, line(6));
      inData = "";
    }

    //GPU Mem Temp
    if (recieved == 'd'){
      inData.remove(inData.length() - 1, 1);
      displayInfo(inData, GPUMemTemp, 2, 150, line(7));
      inData = "";
    }

    //GPU Hotspot Temp
    if (recieved == 'e'){
      inData.remove(inData.length() - 1, 1);
      displayInfo(inData, GPUHotSpotTemp, 2, 215, line(8));
      inData = "";
    }

    //GPU Core Clock
    if (recieved == 'f'){
      inData.remove(inData.length() - 1, 1);
      displayInfo(inData, GPUCoreClock, 4, 180, line(10));
      inData = "";
    }

    //GPU Memory Usage
    if (recieved == 'g'){
      inData.remove(inData.length() - 1, 1);
      displayInfo(inData, GPUMemUsage, 2, 180, line(11));
      inData = "";
    }

    //CPU Usage
    if (recieved == 'h'){
      inData.remove(inData.length() - 1, 1);
      displayInfo(inData, CPULoad, 2, 360, line(1));
      inData = "";
    }

    //GPU Usage
    if (recieved == 'i'){
      inData.remove(inData.length() - 1, 1);
      displayInfo(inData, GPULoad, 2, 360, line(2));
      inData = "";
    }
  }
}
