int count = 0;

enum receiver_state {
  IDLE, //waiting for sync
  DATA //Synced receiving DATA
};
String str_buffer = "";
unsigned int int_buffer; // EXIT/START/8bits DATA/EXIT
unsigned int bit_counter = 0;
unsigned int data_count = 0;
enum receiver_state state = IDLE ;

//This defines receiver properties
#define SENSOR_PIN A3
#define CATHODE_PIN A4
#define SYMBOL_HZ 10
#define SAMPLE_PER_SYMBOL 1
#define WORD_LENGTH 10 // a byte is encoded as a 10-bit value with start and stop bits
#define ETX 0x00 // End of frame symbol
#define STX 0x01 //Start or frame symbol 

#define CPU_HZ 48000000
#define TIMER_PRESCALER_DIV 1024

int sample_signal();
void add_to_buffer(unsigned int edge);
int is_buffer_valid_word();

// global variables for frame decoding
int buffer_index  = 0;
int buffer_size = 10;

//state variables of the thresholder
unsigned int signal_mean = 0 ;
unsigned long acc_sum = 0 ; //used to compute the signal mean value
unsigned int acc_counter = 0 ;

//manechester decoder state variable
long shift_reg = 0;

//----------------------------------BEGIN TIMER SETUP----------------------------------

void setTimerFrequency(int frequencyHz) {
  int compareValue = (CPU_HZ / (TIMER_PRESCALER_DIV * frequencyHz)) - 1;
  TcCount16* TC = (TcCount16*) TC3;
  // Make sure the count is in a proportional position to where it was
  // to prevent any jitter or disconnect when changing the compare value.
  TC->COUNT.reg = map(TC->COUNT.reg, 0, TC->CC[0].reg, 0, compareValue);
  TC->CC[0].reg = compareValue;
  //Serial.println(TC->COUNT.reg);
  //Serial.println(TC->CC[0].reg);
  while (TC->STATUS.bit.SYNCBUSY == 1);
}

void startTimer(int frequencyHz) {
  REG_GCLK_CLKCTRL = (uint16_t) (GCLK_CLKCTRL_CLKEN | GCLK_CLKCTRL_GEN_GCLK0 | GCLK_CLKCTRL_ID_TCC2_TC3) ;
  while ( GCLK->STATUS.bit.SYNCBUSY == 1 ); // wait for sync

  TcCount16* TC = (TcCount16*) TC3;

  TC->CTRLA.reg &= ~TC_CTRLA_ENABLE;
  while (TC->STATUS.bit.SYNCBUSY == 1); // wait for sync

  // Use the 16-bit timer
  TC->CTRLA.reg |= TC_CTRLA_MODE_COUNT16;
  while (TC->STATUS.bit.SYNCBUSY == 1); // wait for sync

  // Use match mode so that the timer counter resets when the count matches the compare register
  TC->CTRLA.reg |= TC_CTRLA_WAVEGEN_MFRQ;
  while (TC->STATUS.bit.SYNCBUSY == 1); // wait for sync

  // Set prescaler to 1024
  TC->CTRLA.reg |= TC_CTRLA_PRESCALER_DIV1024;
  while (TC->STATUS.bit.SYNCBUSY == 1); // wait for sync

  setTimerFrequency(frequencyHz);

  // Enable the compare interrupt
  TC->INTENSET.reg = 0;
  TC->INTENSET.bit.MC0 = 1;

  NVIC_EnableIRQ(TC3_IRQn);

  TC->CTRLA.reg |= TC_CTRLA_ENABLE;
  while (TC->STATUS.bit.SYNCBUSY == 1); // wait for sync
}
  
void TC3_Handler() {
  TcCount16* TC = (TcCount16*) TC3;
  // If this interrupt is due to the compare register matching the timer count
  // we toggle the LED.
  if (TC->INTFLAG.bit.MC0 == 1) {
    TC->INTFLAG.bit.MC0 = 1;
    // Write callback here!!!
    sample_signal();
  }
}
//----------------------------------END TIMER SETUP----------------------------------

void setup() {
  // initialize serial communication at 115200 bits per second:
  int i; 
  Serial.begin(115200);
  Serial.println("Start of receiver program");
  pinMode(13, OUTPUT);
  pinMode(13, HIGH);
  pinMode(CATHODE_PIN, OUTPUT);
  // pinMode(SENSOR_PIN, INPUT); // testing odd behavior
  digitalWrite(SENSOR_PIN, LOW);
  digitalWrite(CATHODE_PIN, LOW);
  startTimer(SYMBOL_HZ * SAMPLE_PER_SYMBOL);
}

int readLED()
{
  pinMode(SENSOR_PIN, OUTPUT);
  digitalWrite(SENSOR_PIN, HIGH);

  pinMode(SENSOR_PIN, INPUT);
  //digitalWrite(SENSOR_PIN, LOW);

  return analogRead(SENSOR_PIN);
}

#define DIF_THRESHOLD 50
int upper = 0;
int lower = 0;

#define BYTE_MASK 0xFF  // keep first 8 bits
int sample_signal(){ 
  int read_val = -1;
  int valid_read = 0;
  int sensorValue = readLED();
  int isBufferWord = 0;
  char byte_word;

  
  Serial.print("Read ");
  Serial.print(sensorValue);
  Serial.print(" upper ");
  Serial.print(upper);
  Serial.print(" lower ");
  Serial.print(lower);
  
  
  if((sensorValue - upper) > DIF_THRESHOLD) upper = sensorValue;
  if((upper - sensorValue) > DIF_THRESHOLD) lower = sensorValue;

  if((sensorValue - upper) < DIF_THRESHOLD && (upper - sensorValue) < DIF_THRESHOLD) read_val = 0;
  if((sensorValue - lower) < DIF_THRESHOLD && (lower - sensorValue) < DIF_THRESHOLD) read_val = 1;
  valid_read = (upper - lower) > DIF_THRESHOLD ? 1 : 0;

  
  Serial.print(" ");
  Serial.println(read_val);
  
  // desync, reset
  // without this the program cannot gracefully recover from desyncs that involve lower and upper meeting
  if(((upper - lower) < DIF_THRESHOLD || read_val == -1) && (upper != 0 && lower != 0)) {
    Serial.println("RESET TRIPPED");
    upper = 0;
    lower = 0;
    state = IDLE;
    bit_counter = 0;
    data_count = 0;
    int_buffer = 0; // empty buffer
    //clear buffer and set state as desynced
  } else { // valid read
    add_to_buffer(read_val);

    
    Serial.print(bit_counter);
    Serial.print(" ");
    
    Serial.println(int_buffer,BIN);
    isBufferWord = is_buffer_valid_word();
    if (isBufferWord == 1 && state == DATA) { // do something with data
      if(((bit_counter) % WORD_LENGTH) == 0) {
        Serial.println("DATA!");
        str_buffer += (int_buffer >> 1) & BYTE_MASK; //right shift by 1 to remove start bit and get first 8 bits for byte data        
        data_count ++;
      }
    } else if (isBufferWord == 2) { // we are now synced and ready to receive data
      Serial.println("SYNC");
      bit_counter = 0;
      data_count = 0;
      str_buffer = ""; // clear string buffer for new data
      state = DATA;
    } else if (isBufferWord == 3) { // store data
      state = IDLE;
    }
    if(state == DATA) {
      bit_counter++;
    }
  }
  
  
}


#define WORD_MASK 0x7FF // keep first 11 bits
inline void add_to_buffer(unsigned int bit) {
  int_buffer = ((int_buffer << 1) | bit) & WORD_MASK; //left shift by 1 and store new bit at first position. Then apply mask to keep only first 11 bits
}

#define SYNC_SYMBOL 0b1000000001
#define START_SYMBOL 0b1 // 10 high to low
#define EXIT_SYMBOL 0b0  // 01 low to high
#define FIRST_ESDE_MASK ((START_SYMBOL << 10) | (START_SYMBOL << 9) | EXIT_SYMBOL)
#define ESDE_MASK ((EXIT_SYMBOL << 10) | (START_SYMBOL << 9) | EXIT_SYMBOL) //EXIT/START/8 bits DATA/EXIT
                                                                             //  0  / 1  / ZZZZZZZZ  / 0
#define STOP_SYMBOL 0b0111111110
#define WORD_MASK 0b1111111111

inline int is_buffer_valid_word() {
  if((((int_buffer & ESDE_MASK) == ESDE_MASK) || (((int_buffer & FIRST_ESDE_MASK) == FIRST_ESDE_MASK) && data_count == 0)) && state == DATA)
  {
    //Serial.println(ESDE_MASK, BIN);
    Serial.println("DATA MASK MATCH");
    return 1;
  } else if (((int_buffer & WORD_MASK) == SYNC_SYMBOL) && state == IDLE) { // clear string and get ready for new data
    Serial.println("SYNC MASK MATCH");
    return 2; // if state was IDLE then complete on SYNC procedure
  } else if (((int_buffer & WORD_MASK) == STOP_SYMBOL) && state == DATA) { // do w/e with completed message
    Serial.println("STOP MASK MATCH");
    return 3;
  }
  return -1; // current buffer is not valid data
}

void loop() {
  
}
