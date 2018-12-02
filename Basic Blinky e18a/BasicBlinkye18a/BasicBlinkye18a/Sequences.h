/*
 * Sequences.h
 *
 * Created: 11/8/2018 9:21:39 AM
 *  Author: techi
 */ 


#ifndef SEQUENCES_H_
#define SEQUENCES_H_

typedef struct {
	uint8_t inst;
	uint8_t rate;
	uint8_t dur;
	uint8_t bright;
} instruction;

instruction testSeq0[200] =   {{0x00, 4, 1, 255},
								{0x20, 1, 1, 255},
								{0x11, 8, 1, 255},
								{0x50, 8, 1, 255},
								{0x51, 4, 1, 255},
								{0x80, 2, 1, 255},
								{0x81, 8, 1, 255}};
	
instruction testSeq9[200] =    {{0x11, 1, 1, 255}, 
								{0x61, 1, 1, 255},
								{0xb1, 1, 1, 255}};


instruction testSeq4[200] =    {{0x00, 4, 4, 200},
								{0x00, 4, 4, 0}, 
								{0x01, 4, 2, 200},
								{0x40, 4, 2, 0},
								{0x40, 4, 4, 200},
								{0x41, 4, 4, 0},
								{0x80, 4, 6, 0},
								{0x81, 4, 4, 200}};
									
instruction testSeq1[200] = {{0x11, 1, 1, 255}, // 0000 0001 0010 0011
							 {0x51, 1, 1, 0}, // 0110 0101 0100 0111
							 {0xb1, 1, 1, 0}}; // 1000 1001 1010 1011		
								 				 
instruction testSeq2[200] = {{0x21, 1, 4, 200},
							 {0x51, 1, 4, 200},
							 {0x81, 1, 4, 200}};
	
instruction testSeq3[200] = {{0x11, 1, 4, 0},
							 {0x61, 1, 4, 200},
							 {0x91, 1, 4, 0}};

instruction testSeq5[200] = {{0x11, 1, 4, 0},
							 {0x51, 1, 4, 0},
							 {0x91, 1, 4, 200}};
								 
instruction testSeq6[200] = {{0x11, 1, 4, 200},
								{0x51, 1, 4, 200},
								{0x91, 1, 4, 0}};

instruction testSeq7[200] = {{0x11, 1, 4, 0},
								{0x51, 1, 4, 200},
								{0x91, 1, 4, 200}};

instruction testSeq8[200] = {{0x11, 1, 4, 200},
								{0x51, 1, 4, 0},
								{0x91, 1, 4, 200}};

/*instruction testSeq3[200] = {{0x21, 1, 4, 200},
								{0x51, 1, 4, 200},
								{0x81, 1, 4, 200}};
*/									
	
#endif /* SEQUENCES_H_ */