# A proof-of-concept C# Implementation of Dual EC DRBG
#### This implementation is as a proof of concept of how the Dual EC DRBG RNG could be exploited in practice.
#### As part of the implementation, elliptic curve math operations are implemented from scratch (altough simply and slowly), such as point multiplication/addition on an elliptic curve modulo a prime.

### The program works as follows:
1) Loads EC parameters from a json file located in the application's resources (or provided by the proccess' arguments). The json file needs to be formatted like the ones in https://neuromancer.sk/std/.
2) Generates points P and Q on the curve such that Q is just a random point on the curve and, P is e\*Q where e is a secret number. This essentialy creates a backdoor which allows us to get from r\*Q to r\*P when trying to get at the internal state of the RNG from the output in step 4 (r\*P = r\*(Q\*e) = e\*(r\*Q)).
3) Generatess random data using the Dual EC DRBG algorithm and a random seed.
  ![alt text](https://i.imgur.com/ArrOz5d.png "RNG Algorithm Explanation (Hebrew)")
 
  * The algorithm works as follows: we put a random seed in s - a number which represents the current state of the RNG.
  * When generating random data, we calculate the point s\*P, take the X value and call it r.
  * Then, we take the X value of r\*Q, trim 16 bits of that and that is our generated random output!
  * To generate a new state "s", we calculate the X value of of r\*P and put it into s.
  * In this program we generate 70 bytes of random data, which are two full "rounds" of the algorithm plus another one which we only see 10 bytes of the output.
  
4) Tries to guess the state of the RNG using the random data outputted
  ![alt text](https://i.imgur.com/jHqlEw6.png "RNG Algorithm Backdoor Explanation (Hebrew)")
  * Firstly we guess what the X value of r\*Q could be. r\*Q is just the output of the RNG of one "round", only that the RNG trims 16 bits so we need to go through each option of what those 16 bits could be.
  * Foreach guess, we check if there exists a point for that X value, and for each point r\*Q we get the point r\*P by multiplying the secret value e.
  * Foreach r\*P we get the X value, and use that for a seed for a new RNG we create. If we guessed the the right trimmed bits, we should get a RNG that
has the next state after generating the first "round" of bytes.
  * Generate the next round of bytes using the RNG we just found, if it matches the random data outputted in step 2 then we have successfully found the state of the RNG
  (NOTE: in this implementation we use the entire output of the second round to check if the RNG we "guessed" matches the one we want. In practice you can only check if it matches the first 2 bytes instead of the entire round output. This greatly reduces how many bytes we need for exploiting this RNG, instead of needing two whole rounds of output, one round + 2 bytes of the second is enough)
  * "Skip" the next bytes generated until we get to the state where the "original" RNG is after step 2.
  * NOTE: The whole guessing process is done in parallel to speed things up.
5) Generates random data using the RNG from step 2 and the RNG we recovered from the output in step 4 and checks if it matches. It should :)



### **Example for program output**

#### 8 bits trimmed:

![Dual EC Backdoor Example Output (8 bits trimmed)](https://user-images.githubusercontent.com/21063634/194768005-3b0d9e77-10f8-4a09-a925-2c0cde9957c8.png)

#### 16 bits trimmed:

![Dual EC Backdoor Example Output (16 bits trimmed)](https://user-images.githubusercontent.com/21063634/194768011-c7671eab-c577-42a2-987d-fd8f7912314e.png)
