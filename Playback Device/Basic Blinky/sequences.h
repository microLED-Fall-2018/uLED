typedef struct {
	uint8_t inst;
	uint8_t rate;
	uint8_t dur;
	uint8_t bright;
} instruction;

instruction shortDemo[200] =   {{0x00, 4, 1, 255},
								{0x20, 1, 1, 255},
								{0x11, 8, 1, 255},
								{0x40, 8, 1, 255},
								{0x41, 4, 1, 255},
								{0x80, 2, 1, 255},
								{0x81, 8, 1, 255}};
	
								/*{0x11, 1, 1, 255}, 
								{0x61, 1, 1, 255},
								{0xb1, 1, 1, 255}};*/


								/*{{0x00, 4, 4, 200},
								{0x00, 4, 4, 0}, 
								{0x01, 4, 2, 200},
								{0x40, 4, 2, 0},
								{0x40, 4, 4, 200},
								{0x41, 4, 4, 0},
								{0x80, 4, 6, 0},
								{0x81, 4, 4, 200}};*/