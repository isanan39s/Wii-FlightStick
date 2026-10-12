#include <windows.h> 
#include <process.h>
#include <stdio.h>
#include <stdlib.h>
#include <conio.h>
#include <string.h> 

#define FUNC __declspec(dllimport) __stdcall
extern "C" HANDLE FUNC OpenHidHandle(unsigned short vendor_id, unsigned short product_id);
extern "C" void FUNC ReadReport(HANDLE handle,unsigned char *InputReport,int *len);
extern "C" void FUNC WriteReport(HANDLE handle,unsigned char *OutputReport, int *len);
extern "C" void FUNC CloseHidHandle(HANDLE handle);

HANDLE hRsDevHandle;

const USHORT VID = 0x057e; // Nintendo (?)
const USHORT PID = 0x0306; // Wii-Remote (?)

#define MAXREPORTSIZE 256
int		InputLength,OutputLength;
unsigned char	InputReport[MAXREPORTSIZE];
unsigned char	OutputReport[MAXREPORTSIZE];

unsigned char	button1,button2;
unsigned char	Ax,Ay,Az;
unsigned char	wii_remote_loop_flag;
unsigned char	wii_remote_in_use;
int				Ix1,Iy1,Ix2,Iy2;
int				Ix3,Iy3,Ix4,Iy4;

unsigned char buf[32];

void Wii_Remote_Input(void);
void Wii_Remote_mode_0x31(void);
int Wii_Remote_check(void);

void Sleep2(int msec)
{
	LARGE_INTEGER f;
	LARGE_INTEGER t1,t2;
	double t;
	QueryPerformanceFrequency(&f);
	QueryPerformanceCounter(&t1);
	while (1) {
		QueryPerformanceCounter(&t2);
		t = (double)t2.QuadPart - (double)t1.QuadPart;
		t /= (double)f.QuadPart;
		t *= 1000;
		if (t>(double)msec) { return; }
	}
}

void Report_0x11(unsigned char data)
{
	OutputReport[0]=0x11; // Report ID
	OutputReport[1]=data;
	WriteReport( hRsDevHandle , OutputReport , &OutputLength);
}

void Report_0x12(unsigned char data1,unsigned char data2)
{
	OutputReport[0]=0x12; // Report ID
	OutputReport[1]=data1;
	OutputReport[2]=data2;
	WriteReport( hRsDevHandle , OutputReport , &OutputLength);
}

void Report_0x13(unsigned char data)
{
	OutputReport[0]=0x13; // Report ID
	OutputReport[1]=data;
	WriteReport( hRsDevHandle , OutputReport , &OutputLength);
}

void Report_0x16_type1(unsigned char adr,unsigned char len,unsigned char d1,unsigned char d2)
{
	OutputReport[0]=0x16; // Report ID
	OutputReport[1]=0x04; // 00(EEPROM) or 0x04(other i2c device)  or 0x08(other type2)
	OutputReport[2]=adr; // I2C ADRS
	OutputReport[3]=0xFF; // ignore
	OutputReport[4]=d1; // mem adr or command
	OutputReport[5]=len-1;
	OutputReport[6]=d2; // data
	OutputReport[7]=0x00;
	OutputReport[8]=0x00;
	OutputReport[9]=0x00;
	OutputReport[10]=0x00; OutputReport[11]=0x00; OutputReport[12]=0x00; OutputReport[13]=0x00;
	OutputReport[14]=0x00; OutputReport[15]=0x00; OutputReport[16]=0x00; OutputReport[17]=0x00;
	OutputReport[18]=0x00; OutputReport[19]=0x00; OutputReport[20]=0x00; OutputReport[21]=0x00;
	WriteReport( hRsDevHandle , OutputReport , &OutputLength);
}

void Report_0x16_type1_buf(unsigned char adr,unsigned char len,unsigned char d1,unsigned char *buf)
{
	OutputReport[0]=0x16; // Report ID
	OutputReport[1]=0x04; // 00(EEPROM) or 0x04(other i2c device)  or 0x08(other type2)
	OutputReport[2]=adr; // I2C ADRS
	OutputReport[3]=0xFF; // ignore
	OutputReport[4]=d1; // mem adr or command
	OutputReport[5]=len-1;
	for (int i=0;i<16;i++) { OutputReport[6+i]=0; }
	for (int i=0;i<len-1;i++) {
		OutputReport[6+i]=buf[i]; // data
	}
	WriteReport( hRsDevHandle , OutputReport , &OutputLength);
}

void Report_0x17_type1(unsigned char adr,unsigned char len,unsigned char d1)
{
	OutputReport[0]=0x17; // Report ID  ... Auto Read == buf(?)
	OutputReport[1]=0x04; // 00(EEPROM) or 04(other ... 1byte type)
	OutputReport[2]=adr; // I2C ADRS
	OutputReport[3]=0x00; // ignore?
	OutputReport[4]=d1;  // mem adr or command
	OutputReport[5]=0x00; // length H
	OutputReport[6]=len;  // length L
	WriteReport( hRsDevHandle , OutputReport , &OutputLength);
}

void Wii_Remote_mode_0x30(void)
{
	// Set Report Type
	Report_0x12(0x00,0x30);  // button ONLY Report Mode
}

void Wii_Remote_mode_0x31(void)
{
	// Set Report Type
	Report_0x12(0x00,0x31);  // Acc Sensor + button  Report Mode
}

void Wii_Remote_mode_0x33(void)
{
	// Set Report Type
	Report_0x12(0x00,0x33);  // Acc Sensor + button + IR  Report Mode
}

void Wii_Remote_mode_0x37(void)
{
	// Set Report Type
	Report_0x12(0x00,0x37);  // Acc Sensor + button + IR + ext Report Mode
}

void Wii_Remote_Input(void)
{
	unsigned char r;
	int s;
	
	ReadReport( hRsDevHandle , InputReport, &InputLength);
	
	r = InputReport[0];
	
	if ( (r & 0xF0) == 0x30 )  {
		button1=InputReport[1]; button2=InputReport[2];
		if ( r & 0x01 ) {
			Ax = InputReport[3];
			Ay = InputReport[4];
			Az = InputReport[5];
		}
		if ( r & 0x02 ) {
			Ix1 = InputReport[6];
			Iy1 = InputReport[7];
			s   = InputReport[8];
			Ix1 += (s & 0x30) <<4;
			Iy1 += (s & 0xC0) <<2;
			
			Ix2 = InputReport[9];
			Iy2 = InputReport[10];
			s   = InputReport[11];
			Ix2 += (s & 0x30) <<4;
			Iy2 += (s & 0xC0) <<2;
		}
		if ( r & 0x04 ) {
			InputReport[16] = (InputReport[16] ^ 0x17 ) + 0x17;
			InputReport[17] = (InputReport[17] ^ 0x17 ) + 0x17;
			InputReport[18] = (InputReport[18] ^ 0x17 ) + 0x17;
			InputReport[19] = (InputReport[19] ^ 0x17 ) + 0x17;
			InputReport[20] = (InputReport[20] ^ 0x17 ) + 0x17;
			InputReport[21] = (InputReport[21] ^ 0x17 ) + 0x17;
		}
	}
}

void Wii_Remote_Input_Main(void *)
{
	// wii remote input thread
	while (wii_remote_loop_flag) {
		wii_remote_in_use=1;
		Wii_Remote_Input();
		Sleep(100);
	}
	wii_remote_in_use=0;
	return;
}

uintptr_t wii_th;

int Wii_Remote_check(void)
{
	Ax=0;
	wii_remote_loop_flag=1;
	wii_th = _beginthread( Wii_Remote_Input_Main , 0 , NULL );
	Sleep2(1000);
	wii_remote_loop_flag=0;
	return (Ax!=0);
}

void Stop_Wii_remote_th(void)
{
	if (wii_remote_in_use==1) {
		TerminateThread( (HANDLE)wii_th , -1); // kill thread  if it stopped
	}
}

void Read_i2c_device_0xA4(int adr)
{
	unsigned char d1,d2,d3,d4,d5;
	Report_0x17_type1( 0xA4 , 16, adr ); // i2c device 0xA6 read , 16bytes
	while (1) {
		ReadReport( hRsDevHandle , InputReport, &InputLength);
		if (InputReport[0]==0x21) { break; }
		if ( ( InputReport[0]==0x30 ) || ( InputReport[0]==0x31 ) ) {
			button1=InputReport[1]; button2=InputReport[2];
		}
	}
	d1=InputReport[3]; // result ssss-ffff  ... ssss receive-size / ffff==0x07 error
	if ((d1&0x0F)==0x07) {
		for (int i=0;i<16;i++) { buf[i]=0; } //error
	} else {
		//d2=InputReport[4]; d3=InputReport[5];  // data-offset
		for (int i=0;i<16;i++) {
			buf[i] = InputReport[i+6];
		}
	}
}

void Read_i2c_device_0xA6(int adr)
{
	unsigned char d1,d2,d3,d4,d5;
	Report_0x17_type1( 0xA6 , 16, adr ); // i2c device 0xA6 read , 16bytes
	while (1) {
		ReadReport( hRsDevHandle , InputReport, &InputLength);
		if (InputReport[0]==0x21) { break; }
		if ( ( InputReport[0]==0x30 ) || ( InputReport[0]==0x31 ) ) {
			button1=InputReport[1]; button2=InputReport[2];
		}
	}
	d1=InputReport[3]; // result ssss-ffff  ... ssss receive-size / ffff==0x07 error
	if ((d1&0x0F)==0x07) {
		for (int i=0;i<16;i++) { buf[i]=0; } //error
	} else {
		//d2=InputReport[4]; d3=InputReport[5];  // data-offset
		for (int i=0;i<16;i++) {
			buf[i] = InputReport[i+6];
		}
	}
}

void i2c_0xA6_device_dump(void)
{
	printf("i2c-0xA6-device-dump  (this looks like memory, but NOT memory)\n");
	for (int ad = 0x00; ad < 0x100 ; ad+= 0x10 ) {
		Read_i2c_device_0xA6( ad );
		printf(" %04X : ",ad );
		for (int i=0;i<16;i++) {
			printf("%02X ",buf[i]);
		}
		printf("\n");
	}
	printf("\n");
}


int main( int argc, char *argv[])
{ 
	printf("###########################################\n");
	printf("#                                         #\n");
	printf("#   Wii-MotionPlus Read Test version 0.1  #\n");
	printf("#                  build 2009-6-21        #\n");
	printf("#                                         #\n");
	printf("###########################################\n");
	printf("\n");
	
	hRsDevHandle = OpenHidHandle(VID,PID);
	if ( hRsDevHandle == INVALID_HANDLE_VALUE) {
		MessageBox(0,"Wii-Remote not found.",0,0);
		return 1;
	}
	
	Wii_Remote_mode_0x37();
	
	if ( Wii_Remote_check() ) {
		Report_0x11(0x10); // LED1 on
		
		// Wii Motion Plus  Reset
		Read_i2c_device_0xA6(0xFA);
		//printf("ID = %02X%02X%02X%02X%02X%02X\n",buf[0],buf[1],buf[2],buf[3],buf[4],buf[5]);
		if ( (buf[0]!=0)||(buf[1]!=0)||(buf[2]!=0xA6)||(buf[3]!=0x20)||(buf[4]!=0)||(buf[5]!=5) ) {
			
			MessageBox(0,"Wii MotionPlus is not connected.",0,0);
			
		} else {
			Report_0x16_type1( 0xA6 , 1 , 0xF0 , 0x55 ); Sleep(200); // Wii MotionPlus RESET (??)
			
			
			i2c_0xA6_device_dump();
			
			//MainLoop();
			
			Wii_Remote_mode_0x30();
			
		}
		
	} else {
		
		MessageBox(0,"Wii-Remote cannot open.",0,0);
		Stop_Wii_remote_th();
		
	}
	
	CloseHidHandle( hRsDevHandle );
	
	printf("[‰½‚©ƒL[‚ð‰Ÿ‚µ‚Ä‰º‚³‚¢B]\n");
	getch();
	
	return 0;
} 
