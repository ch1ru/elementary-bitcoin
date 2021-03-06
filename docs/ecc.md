[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)

# Elliptic Curve Cryptography

This chapter will be about the maths behind bitcoin keys and signing. Feel free to skip this chapter, it's just for extra insight into cryptographic operations within the bitcoin protocol. 

## What is an elliptic curve? 

An elliptic curve is any curve with the general formula:

y² = ax³ + ax + b

For example, Bitcoin uses the curve equation y² = x³ + 7 which is defined in NIST's list of secure elliptic curves. An interesting property of elliptic curves is that they display intuitive properties we see in standard maths, namely: associativity and commutativity.

If an elliptic curve is intersected by a straight line, unless is some exceptions, it will intersect at 3 points, such that: 

Point A + point B = point C reflected on the x axis. 

Here's an example:

![elliptic curve](/assets/ellipticcurve1.png)

P + Q = R.

We can also intersect at the tangent (P) then use differentiation to find the next intersection (-2P) then reflect over the x-axis to get 2P:

![elliptic curve tangent](/assets/ellipticcurve2.jpeg)

**The maths for the last diagram**

We are trying to find the point of the next intersect (x3, y3) or -R in the diagram.
```
Equation of curve: y² = ax³ + ax + b

Find differentiation equation: 

dy 2y = 3x² + a dx

dy/dx 2y = 3x² + a

dy/dx = (3x² + a)/2y

Gradient, m = (3(p.x)² + a) / 2(p.y)

Using Fermat's little theorem, we know that: x3 = m² - 2(p.x), So we can calculate the x-coordinate x3

We also know that y3 = (m * (p.x - x3)) - p.y, so we now know -R!

We can reflect over the x-axis so negate y3.
```

## So how are elliptic curve keys made?

We Mentioned previously that a public key is a point on an elliptic curve. The process of generating it first involves randomly generating a 256-bit value, which is the private key. You can think of this as a scalar, or point multiplyer. Technically we can't do multiplication on the curve (how do we find the product of 2 points?) but we can do point addition many times. Finding 2P (The process of drawing a tangent at P like we explained above) is the same as P + P. 5P is P + P + P + P + P etc. Easy right? So the public key is the multiplication of a known point, G, and the 256-bit random number. 

## How big is the private key?

256 bits can be stored in a 32-byte hex string. Despite this, 2^256 is insanely huge. To put it into perspective, It is thought that the milky way may contain around 10^67 atoms. However this is only a fraction of the number of possible elliptic curve private keys! Clearly, in a crypto system which calculates the public key by hopping around a curve the number of times there are atoms in thousands of galaxies is not very useful to our current hardware limits! It turns out there is a nifty trick called binary expansion which can much reduce the problem and make it computationally trivial to solve.

## Elliptic curves within finite fields

All the maths we have covered so far is all useful and correct(ish), however it's not a full cryptosystem yet. The curve is actually bounded by a finite field. All the operations we have covered do indeed take place, but within a prime field. The equation that bitcoin uses is called Secp256k1, its equation is actually: y^2 = x³ + 7 mod P, where P is a very large prime (2^256 - 2^32 - 977 to be exact). 

This gives the curve some interesting properties. Visually, it no longer looks like a curve but a series of scattered points.

![Elliptic curve mod](/assets/ellipticcurve3.png)

Interestingly, all the mathmatic identities and properties like associativity and commutavity remain unchanged. We can calculate everything as we did before but for every basic operation, it also requires a modular operation of the large prime (% P).

## Finding the last missing group axiom

A group in a cryptosystem requires at least these 4 properties:

- Associativity - (A + B) + C ≡ A + (B + C). This isn't immediately obvious in elliptic curves, but look at the images below. You will see that (A + B) + C is indeed the same as (B + C) + A. 

(A + B) + C:
![associativity](/assets/associative1.png)

(B + C) + A:
![associativity](/assets/associative2.png)

- Closure - For any x and y in the group, x * y is also in the group. This is true for elliptic curves since no matter what our scalar is (with some exceptions that we discard     in the event of the 256-bit number being higher than the order N, which is slightly smaller) the point will always stay in the field.  
- Inverse element - For every x there is -x where both are in the group. 
- Identity element - This is an element that provides us with an identity such that e * x ≡ x * e ≡ x. In a multiplicative group the identity will be 1 (a * 1 = a). In an        additive group (elliptic curves falls under this group, no multiplication remember!) you can think of the identity as being 0 (a + 0 = a). Have a pause now to think about what the identity element could be in elliptic curves? 

In addition to these there is commutativity, which we mentioned previously. A group is commutative if x * y ≡ y * x. This is easy to prove in elliptic curves; a line intersecting P and Q will produce the same result no mater the order.

A group is called cyclic if there is an element, G, which spans all elements in the group (g, g², g³ ...etc). In elliptic curves, this is also true since the base point G of the curve will span all points in the additive group (G, 2G, 3G % P).

All these properties come organically within the elliptic curve cryptosystem except one: the identity element. Which point can we add to another to get its identity? There is none (It was a trick question when I asked what the identity element could be!). To combat this, there is an imaginary point called the point at infinity which, when added to a point, will get its identity. We can visualise this by drawing a vertical line down the elliptic curve. The line will intersect at exactly 2 places. We also say it intersects at a third: the point at infinity. Following the rules we have established so far, when we add the point at infinity to P, the line intersects at one other place, at -P. Reflecting this back across the x-axis gives us P. Thus we have found our missing axiom! (albeit a slightly convuluted and perhaps uintuitive sense).

## What is the public key?

The public key is the coordinate resulting from the base point, G, multiplied by the private key. So the public key can be the x and y coordinate in hex. However, these values are 32-bytes each which makes storing the public key as 64 bytes a pain. Instead, we can just store the x-coordinate, since we know the equation and can derive y. However, the definition we have for y is y² = x³ + 7 (mod P). By square-rooting both sides we can find one solution for y. But don't forget, a negative value of y would also yield the same when squared, so we have to find the negative solution. How do we do this?

Let's start with what we know: there are 2 solutions for y, one above the x-axis and one below. 

The equation of the curve has a modulo function either side: y % P = +/-√(x³ + 7) % P. A correct solution would be (x,y) or (x,-y) that satisfies this equation.

We also know know from Fermat's little theorem that -y % P ≡ (P - y) % P. 

Since we know the prime number is odd, and we know one value for y by solving the above equation (y % P = +/-√(x³ + 7) % P). If y is even, then we know that P-y is odd. If y is odd then P-y is even. So all we need to know is what solution of y we have, whether is it even or odd.

So our public key becomes:
[1 byte eveness of y][32-byte x]

even = 0x02, odd= 0x03

Example: 02b4632d08485ff1df2db55b9dafd23347d1c47a457072a1e87be26896549a8737

Using this format called SEC compressed, we can tell which solution we are using for y with only one extra byte!

## Let's summarize the whole process

If you didn't get some of the maths, not to worry! this summary will provide you with the most important points. The rest of this tutorial will not require a deeper understanding than the basic concept of public key cryptography with signing and verification.

**Key generation:**
- Create a 256-bit value from random entropy
- Multiply a known point, G, by this scalar to get the public key point
- The exquation of the curve is y² = x³ + 7 mod P, where P is a large prime number
- All calculations are done within the prime field of P
- Binary expansion is used for fast exponentiation (G -> 2G -> 4G etc uses O(n) time)
- The public key is the coordinate, although only the x-coordinate needs to be stored plus one byte if y is even (0x02) or odd (0x03)

**Signing:**



[/Intro](/index.md)|[/Install](/install.md)|[/keys](/keys.md)|[/Crypto](ecc.md)|[/Wallet](wallet.md)|[/Transactions](transactions.md)|[/Script](script.md)|[/Blocks](blocks.md)|[/Mining](/mining.md)|[/SPV](spv.md)|[/Segwit](segwit.md)
