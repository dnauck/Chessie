﻿using Chessie.ErrorHandling;
using Chessie.ErrorHandling.CSharp;
using Microsoft.FSharp.Collections;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chessie.CSharp.Test
{
    // originally from https://github.com/fsprojects/fsharpx/blob/master/tests/FSharpx.CSharpTests/ValidationExample.cs

    enum Sobriety { Sober, Tipsy, Drunk, Paralytic, Unconscious }
    enum Gender { Male, Female }

    class Person
    {
        public Gender Gender { get; private set; }
        public int Age { get; private set; }
        public List<string> Clothes { get; private set; }
        public Sobriety Sobriety { get; private set; }

        public Person(Gender gender, int age, List<string> clothes, Sobriety sobriety)
        {
            this.Gender = gender;
            this.Age = age;
            this.Clothes = clothes;
            this.Sobriety = sobriety;
        }
    }

    class Club
    {
        public static Result<Person, string> CheckAge(Person p)
        {
            if (p.Age < 18)
                return Result<Person, string>.FailWith("Too young!");
            if (p.Age > 40)
                return Result<Person, string>.FailWith("Too old!");
            return Result<Person, string>.Succeed(p);
        }

        public static Result<Person, string> CheckClothes(Person p)
        {
            if (p.Gender == Gender.Male && !p.Clothes.Contains("Tie"))
                return Result<Person, string>.FailWith("Smarten up!");
            if (p.Gender == Gender.Female && p.Clothes.Contains("Trainers"))
                return Result<Person, string>.FailWith("Wear high heels!");
            return Result<Person, string>.Succeed(p);
        }

        public static Result<Person, string> CheckSobriety(Person p)
        {
            if (new[] { Sobriety.Drunk, Sobriety.Paralytic, Sobriety.Unconscious }.Contains(p.Sobriety))
                return Result<Person, string>.FailWith("Sober up!");
            return Result<Person, string>.Succeed(p);
        }
    }

    class ClubbedToDeath
    {
        public static Result<decimal, string> CostToEnter(Person p)
        {
            return from a in Club.CheckAge(p)
                   from b in Club.CheckClothes(a)
                   from c in Club.CheckSobriety(b)
                   select c.Gender == Gender.Female ? 0m : 5m;
        }
    }

    [TestFixture]
    class Test1
    {
        [Test]
        public void Part1()
        {
            var Dave = new Person(Gender.Male, 41, new List<string> { "Tie", "Jeans" }, Sobriety.Sober);
            var costDave = ClubbedToDeath.CostToEnter(Dave);
            Assert.AreEqual("Too old!", costDave.FailedWith().First());

            var Ken = new Person(Gender.Male, 28, new List<string> { "Tie", "Shirt" }, Sobriety.Tipsy);
            var costKen = ClubbedToDeath.CostToEnter(Ken);
            Assert.AreEqual(5m, costKen.SucceededWith());

            var Ruby = new Person(Gender.Female, 25, new List<string> { "High heels" }, Sobriety.Tipsy);
            var costRuby = ClubbedToDeath.CostToEnter(Ruby);
            costRuby.Match(
                (x, msgs) =>
                {
                    Assert.AreEqual(0m, x);
                },
                msgs =>
                {
                    Assert.Fail();

                });

            var Ruby17 = new Person(Ruby.Gender, 17, Ruby.Clothes, Ruby.Sobriety);
            var costRuby17 = ClubbedToDeath.CostToEnter(Ruby17);
            Assert.AreEqual("Too young!", costRuby17.FailedWith().First());

            var KenUnconscious = new Person(Ken.Gender, Ken.Age, Ken.Clothes, Sobriety.Unconscious);
            var costKenUnconscious = ClubbedToDeath.CostToEnter(KenUnconscious);
            costKenUnconscious.Match(
                (x, msgs) =>
                {
                    Assert.Fail();
                },
                msgs =>
                {
                    Assert.AreEqual("Sober up!", msgs.First());
                });
        }
    }

    class ClubTropicana
    {
        public static Result<decimal, string> CostToEnter(Person p)
        {
            return from c in Club.CheckAge(p)
                   join x in Club.CheckClothes(p) on 1 equals 1
                   join y in Club.CheckSobriety(p) on 1 equals 1
                   select c.Gender == Gender.Female ? 0m : 7.5m;
        }

        public static decimal CostByGender(Person p, Person x, Person y)
        {
            return p.Gender == Gender.Female ? 0m : 7.5m;
        }

        public static Result<decimal, string> CostToEnter2(Person p)
        {
            return new Func<Person, Person, Person, decimal>(CostByGender)
                .Curry().ReturnValidation()
                .Apply(Club.CheckAge(p))
                .Apply(Club.CheckClothes(p))
                .Apply(Club.CheckSobriety(p));
        }
    }

    [TestFixture]
    class Test2
    {
        [Test]
        public void Part2()
        {
            var daveParalytic = new Person(
                age: 41,
                clothes: new List<string> { "Tie", "Shirt" }, 
                gender: Gender.Male,
                sobriety: Sobriety.Paralytic);
            
            var costDaveParalytic = ClubTropicana.CostToEnter(daveParalytic);

            costDaveParalytic.Match(
                ifSuccess: (x, msgs) => Assert.Fail(),
                ifFailure: errs => Assert.That(errs.ToList(), Is.EquivalentTo(new[] { "Too old!", "Sober up!" })));
            
            var ruby = new Person(Gender.Female, 25, new List<string> { "High heels" }, Sobriety.Tipsy);
            var costRuby = ClubTropicana.CostToEnter2(ruby);
            
            costRuby.Match(
                ifSuccess: (x, msgs) =>
                {
                    Assert.AreEqual(0m, x);
                    Assert.That(msgs, Is.EquivalentTo(new List<string>()));
                },
                ifFailure: errs => Assert.Fail());
        }
    }

    class GayBar
    {
        public static Result<Person, string> CheckGender (Person p)
        {
            if (p.Gender == Gender.Male)
                return Result<Person, string>.Succeed(p);
            return Result<Person, string>.FailWith("Men only");
        }

        public static Result<decimal, string> CostToEnter(Person p)
        {
            return new List<Func<Person, Result<Person, string>>> { CheckGender, Club.CheckAge, Club.CheckClothes, Club.CheckSobriety }
                .Select(check => check(p))
                .Collect()
                .Select(x => x[0].Age + 1.5m);
        }
    }

    [TestFixture]
    class Test3
    {
        [Test]
        public void Part3()
        {
            var person = new Person(
                gender: Gender.Male,
                age: 59,
                clothes: new List<string> { "Jeans" },
                sobriety: Sobriety.Paralytic);
            var cost = GayBar.CostToEnter(person);
            cost.Match(
                ifSuccess: (x, msgs) => Assert.Fail(),
                ifFailure: errs => Assert.That(errs, Is.EquivalentTo(new[] { "Too old!", "Smarten up!", "Sober up!" })));
        }
    }
}