# A naive proof-of-concept C# Implementation of Dual EC DRBG
This implementation works as a proof of concept of the whole DUAL EC DRBG backdoor.
In here, some simple Elliptic curve math operations are implemented from scratch as well.
The program works as follows:
1) Loads a curve from a json file in resources (or in arguments)
2) Generate points P and Q on the curve such that Q is just a random point and, P = e*Q where e is a secret number. This essentialy creates a backdoor which allows us to get from r*Q to r*P when trying to get at the internal state of the RNG from the output (in step 4).
3 Generatess random data using the Dual EC DRBG algorithm and a random seed.
3a) The algorithm is as follows: we put a random seed in s, a number which represents the current state of the RNG
3b) When generating random data we calculate the point s*P, take the X value and call it r
3c) Then, we take the X value of r*Q, trim 16 bits of that and that is our generated random output
3d) To generate a new state "s", we calculate the X value of of r*P and put it into s.
4) Tries to guess the state of the RNG using the random data outputted
4a) Firstly we guess the X value of r*Q could be. r*Q is just the output of the RNG of one "round", only that the RNG trims 16 bits so we need to guess them and go through all of them.
4b) Foreach guess we check for corrosponding points on the curve (each X has two y's), and for each point r*Q we get the point r*P by multiplying
by the secret value e.
4c) Foreach r*P we get the X value, and use that for a seed of a new RNG we create. If we guessed the the right trimmed bits, we should get a RNG that
has the next state after generating the first "round" of bytes (used in 3a).
4d) Generate the next round of bytes using the RNG we just found, if it matches the random data outputted in step 2 then we have successfully found the state of the RNG
4e) "Skip" the next bytes generated until we get to the state where the "original" RNG is after step 2.
5) Generate random data using the RNG from step 2 and the RNG we recovered from the output in step 4 and check if it matches. It should :)
