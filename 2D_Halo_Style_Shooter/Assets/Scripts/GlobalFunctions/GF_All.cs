/*************************************************************************************
My intense hatred of C# is coming to a head. I have to do this in order to have normal functions. Fuck C#.

Three things that piss me off to no end about C#.
1) All functions must be bundled with classes. THis is fucking stupid, and makes every function be able to have crazy
sneaky side effects. This causes my program to be harder to read and way more error prone.
2) All class data passed by reference. This yet again leads to crazy side effects being possible for every fucking 
function that has a reference to anything else. This is fucking stupid.
3) Lack of multiple return types. So now if I have some function that modifies two things I can't just pass in copies, I need
to pass in the whole things by pointers. Or I need to make a new data type just for that one fucking function so that it 
can return a single return type. Fuck this language.

And the worst part is that all of these things build on each other to make each other exponentially shitty. Steps 1 and 2 combine 
to make me desperate to avoid member functions, and try to go for static functions. However, step 3 makes global static functions an 
enormous pain in the ass.
*************************************************************************************/
using UnityEngine;

// I realized that fuck C#.
public static class GF_All
{
}
