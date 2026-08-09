# The C# you actually need

**This is not a C# course, and you should not turn it into one.** The internet teaches C# well, and
you are already good at it. What nothing else knows is *this framework* — spend your words there.

What this page is for: **eight ideas, each anchored to a real file**, so that when an engineer asks
"what is a class?" you can answer by pointing at something in their own model rather than at an
abstraction. Say the modelling meaning first and the C# name second.

Paths below assume a model scaffolded as `MyRoadModel` with `--element RoadSegment`. Substitute
their names.

---

### 1. A class is a kind of thing

`Objects\RoadSegment.cs`

A class describes what one asset *is*: what you know about it, and what it can do. One class,
thousands of segments — the same way one form describes thousands of inspection records.

The properties near the top are the things you know about a segment: its age, its condition, its
area. The methods below are the things it can do: deteriorate, recover from a treatment.

### 2. An object is one of them

`Objects\RoadSegmentFactory.cs`

The factory builds one `RoadSegment` per element in the network, every period. A *class* is the
description; an *object* is one actual segment with actual numbers in it. The framework hands you
raw values in dictionaries and the factory turns them into something with names.

**This is the file where every input column name lives.** If a column name is wrong, it is wrong
here.

### 3. A property is a named value on that thing

`Objects\RoadSegment.cs`

```csharp
public double ConditionRating { get; set; }
```

Read it as: *every road segment has a condition rating, and it is a number*. `double` means "a
number that can have decimals". `int` means "a whole number". `string` means "text". `bool` means
"yes or no".

### 4. A method is something the model does, with a name

`Objects\Incrementer.cs`

A method is a named piece of work you can run. `Increment` is *"make this segment one period
older and worse"*. Whatever is in the round brackets is what it needs to do the job; whatever is
after the arrow or in a `return` is the answer it gives back.

Methods are how the framework talks to a domain model: it calls yours, by name, at the right moment
— [`how-a-run-works.md`](how-a-run-works.md).

### 5. `if` is a rule, and `switch` is a table of rules

`Objects\TreatmentsTrigger.cs`, `Objects\Resetter.cs`

`if (segment.Age > resealAge)` is exactly the sentence an engineer would write in a specification.
A `switch` is the same idea with many branches — one arm per treatment, in `Resetter.cs`.

**A `switch` over treatment names should have a `default:` arm that throws.** Then a treatment
somebody forgets to handle fails loudly instead of silently doing nothing. `jcass-dm check`'s
`treatment reset arms` rule looks for exactly this shape.

### 6. A constant is a number with a name

`Objects\Constants.cs`, `Objects\TreatmentNames.cs`

`TreatmentNames` gives each treatment name one spelling, in one place, used by both the C# and the
bundle. Typing `"reseal"` twice is how the two drift apart.

`Constants` is different and more important: it is where every **tunable** number is read out of the
client's `lookups.xlsx` at setup. The distinction between a number that belongs there and one that
belongs in C# is the single rule most worth teaching —
[`../conventions/where-numbers-live.md`](../conventions/where-numbers-live.md).

### 7. Inheriting means "start from theirs and fill in the gaps"

`Objects\MyRoadModel.cs`

```csharp
public class MyRoadModel : DomainModelBase
```

*"A MyRoadModel is a DomainModelBase, with my bits filled in."* `DomainModelBase` is the framework's
half of the contract; the `override` methods are the gaps you fill. The framework then calls the
methods it already knows exist.

**The signatures are not yours to change.** Alter one and it stops compiling; work around it and the
framework stops finding your class.

### 8. `null` means "there is nothing here"

Not a file — a habit.

`null` is the absence of a value, and it is behind the commonest crash in a domain model. A property
that was never set, a lookup key that does not exist, a text column that was blank in the CSV: all
`null`, and using one throws *"object reference not set to an instance of an object"*. What that
means in practice, and how to find which one it was:
[`reading-errors.md`](reading-errors.md).

---

## What not to teach

`async`, LINQ query syntax, generics, interfaces beyond *"the framework's list of methods you must
write"*, dependency injection, unit-testing frameworks, `IDisposable`, records, pattern matching
beyond a simple `switch`.

None of it is needed to write a good domain model, and every minute spent on it is a minute not
spent on the framework's conventions — which are what will actually bite.

**If a piece of C# is genuinely needed and is not here, explain it in the moment, anchored to the
line in front of them.** Do not send them away to learn a language.
