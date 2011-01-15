About
=====

This is a C# interpretation of Raganwald's [`andand`][1]. It provides a so-called *guarded method invocation* or *safe navigation method*. In other words, you can be blissfully ignorant of any null references when resolving a chain of member accesses, as in `foo.bar.baz`. With `AndAnd`, if `foo` or `foo.bar` resulted in a null reference, a `null` ref would be returned instead of throwing a `NullReferenceException`. Without `AndAnd`, you would have to verify that both `foo` and `foo.bar` result in a non-null reference, otherwise a `NullReferenceException` would be thrown upon invoking `foo.bar` and `foo.bar.baz`, respectively.

Usage
=====

`var result = someObject.AndAnd(obj => obj.Some().Expression._that.Might().Otherwise_Throw_a_NullReferenceException());`

How it Works
============

The `AndAnd` extension method takes in an expression representing the member accesses that you wish to execute and translates the tree such that each member access is first checked for null.

Example
-------

Here's a before and after example of the following expression: `(t => t.OtherThingField.OtherThing)`

**Before:**

    .Lambda #Lambda1<System.Func`2[AndAnd.Tests.Thing,AndAnd.Tests.Thing]>(AndAnd.Tests.Thing $t) {
        ($t.OtherThingField).OtherThing
    }

**After:**

    .Lambda #Lambda1<System.Func`2[AndAnd.Tests.Thing,AndAnd.Tests.Thing]>(AndAnd.Tests.Thing $t) {
        .Block(
            System.Boolean $isSafeToContinue,
            AndAnd.Tests.Thing $var0,
            AndAnd.Tests.Thing $var1) {
            $isSafeToContinue = True;
            .If ($t == null) {
                .Block() {
                    $isSafeToContinue = False;
                    .Return skipToEnd { }
                }
            } .Else {
                .Default(System.Void)
            };
            $var0 = $t.OtherThingField;
            .If ($var0 == null) {
                .Block() {
                    $isSafeToContinue = False;
                    .Return skipToEnd { }
                }
            } .Else {
                .Default(System.Void)
            };
            $var1 = $var0.OtherThing;
            .Label
            .LabelTarget skipToEnd:;
            .If (
                $isSafeToContinue
            ) {
                $var1
            } .Else {
                null
            }
        }
    }


  [1]: http://andand.rubyforge.org/