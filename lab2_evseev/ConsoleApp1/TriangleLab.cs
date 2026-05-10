using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TriangleLab;

namespace TriangleLab.Tests
{
    [TestFixture]
    public class TriangleCalculatorTests
    {
        private TriangleCalculator _calculator;

        [SetUp]
        public void SetUp()
        {
            _calculator = new TriangleCalculator();
        }

        [Test]
        public void Calculate_Equilateral_ReturnsRavnostoronniy()
        {
            var result = _calculator.Calculate("5", "5", "5");
            Assert.That(result.Type, Is.EqualTo("равносторонний"));
            Assert.That(result.Coordinates.Count, Is.EqualTo(3));
            Assert.That(result.Coordinates.All(c => c.X >= 0 && c.X <= 100 && c.Y >= 0 && c.Y <= 100));
        }

        [Test]
        public void Calculate_Isosceles_TwoEqualSides_ReturnsRavnobedrenniy()
        {
            var result = _calculator.Calculate("3", "3", "4");
            Assert.That(result.Type, Is.EqualTo("равнобедренный"));
        }

        [Test]
        public void Calculate_Scalene_AllDifferent_ReturnsRaznostoronniy()
        {
            var result = _calculator.Calculate("3", "4", "5");
            Assert.That(result.Type, Is.EqualTo("разносторонний"));
        }

        [Test]
        public void Calculate_ValidTriangle_CoordinatesAreDistinct()
        {
            var result = _calculator.Calculate("6", "7", "8");
            var distinct = result.Coordinates.Distinct().Count();
            Assert.That(distinct, Is.EqualTo(3), "Точки треугольника должны быть уникальны");
        }

        [Test]
        public void Calculate_ValidTriangle_CoordinatesWithinBounds()
        {
            var result = _calculator.Calculate("100", "100", "100");
            foreach (var p in result.Coordinates)
            {
                Assert.That(p.X, Is.GreaterThanOrEqualTo(0));
                Assert.That(p.X, Is.LessThanOrEqualTo(100));
                Assert.That(p.Y, Is.GreaterThanOrEqualTo(0));
                Assert.That(p.Y, Is.LessThanOrEqualTo(100));
            }
        }

        [TestCase("abc", "4", "5")]
        [TestCase("3", "xyz", "5")]
        [TestCase("3", "4", "")]
        public void Calculate_NonNumeric_ReturnsEmptyTypeAndErrorCoords(string a, string b, string c)
        {
            var result = _calculator.Calculate(a, b, c);
            Assert.That(result.Type, Is.Empty);
            Assert.That(result.Coordinates.Count, Is.EqualTo(3));
            Assert.That(result.Coordinates.All(p => p.X == -2 && p.Y == -2));
        }

        [TestCase("-3", "4", "5")]
        [TestCase("3", "-4", "5")]
        [TestCase("3", "4", "-5")]
        public void Calculate_NegativeSide_ReturnsNotTriangle(string a, string b, string c)
        {
            var result = _calculator.Calculate(a, b, c);
            Assert.That(result.Type, Is.EqualTo("не треугольник"));
            Assert.That(result.Coordinates.All(p => p.X == -1 && p.Y == -1));
        }

        [TestCase("0", "4", "5")]
        [TestCase("3", "0", "5")]
        [TestCase("3", "4", "0")]
        public void Calculate_ZeroSide_ReturnsNotTriangle(string a, string b, string c)
        {
            var result = _calculator.Calculate(a, b, c);
            Assert.That(result.Type, Is.EqualTo("не треугольник"));
            Assert.That(result.Coordinates.All(p => p.X == -1 && p.Y == -1));
        }

        [TestCase("1", "2", "5")]
        [TestCase("5", "1", "2")]
        [TestCase("2", "5", "1")]
        [TestCase("2", "3", "5")]
        public void Calculate_TriangleInequality_ReturnsNotTriangle(string a, string b, string c)
        {
            var result = _calculator.Calculate(a, b, c);
            Assert.That(result.Type, Is.EqualTo("не треугольник"));
        }

        [Test]
        public void Calculate_NullFirstArgument_ShouldNotThrow()
        {
            Assert.DoesNotThrow(() => _calculator.Calculate(null, "4", "5"));
            var result = _calculator.Calculate(null, "4", "5");
            Assert.That(result.Type, Is.Empty);
            Assert.That(result.Coordinates[0], Is.EqualTo((-2, -2)));
        }

        [Test]
        public void Calculate_NullSecondArgument_ShouldNotThrow()
        {
            Assert.DoesNotThrow(() => _calculator.Calculate("3", null, "5"));
            var result = _calculator.Calculate("3", null, "5");
            Assert.That(result.Type, Is.Empty);
        }

        [Test]
        public void Calculate_NullThirdArgument_ShouldNotThrow()
        {
            Assert.DoesNotThrow(() => _calculator.Calculate("3", "4", null));
            var result = _calculator.Calculate("3", "4", null);
            Assert.That(result.Type, Is.Empty);
        }

        [Test]
        public void Calculate_InfinityInput_ReturnsError()
        {
            var result = _calculator.Calculate("Infinity", "4", "5");
            Assert.That(result.Type, Is.Empty);
            Assert.That(result.Coordinates.All(p => p.X == -2 && p.Y == -2));
        }

        [TestCase("3", "4", "5")]
        [TestCase("4", "5", "3")]
        [TestCase("5", "3", "4")]
        public void Calculate_Permutation_TypeRemainsSame(string a, string b, string c)
        {
            var result = _calculator.Calculate(a, b, c);
            Assert.That(result.Type, Is.EqualTo("разносторонний"));
        }

        [Test]
        public void Calculate_LargeEquilateral_Works()
        {
            var result = _calculator.Calculate("1e10", "1e10", "1e10");
            Assert.That(result.Type, Is.EqualTo("равносторонний"));
        }

        [Test]
        public void Calculate_LargeScalene_Works()
        {
            var result = _calculator.Calculate("1e10", "2e10", "1.5e10");
            Assert.That(result.Type, Does.StartWith("разносторонний"));
        }

        [Test]
        public void Calculate_TinyTriangle_Works()
        {
            var result = _calculator.Calculate("0.001", "0.001", "0.001");
            Assert.That(result.Type, Is.EqualTo("равносторонний"));
        }

        [Test]
        public void Calculate_AlmostEquilateral_WithinEpsilon_ReturnsEquilateral()
        {
            var result = _calculator.Calculate("5.000001", "5.0", "5.0");
            Assert.That(result.Type, Is.EqualTo("равносторонний"));
        }

        [Test]
        public void Calculate_AlmostIsosceles_WithinEpsilon_ReturnsIsosceles()
        {
            var result = _calculator.Calculate("3.000001", "3.0", "4.0");
            Assert.That(result.Type, Is.EqualTo("равнобедренный"));
        }

        [Test]
        public void Calculate_Whitespace_IsTrimmedAndParsed()
        {
            var result = _calculator.Calculate("  5 ", " 5", "5  ");
            Assert.That(result.Type, Is.EqualTo("равносторонний"));
        }

        [Test]
        public void Calculate_NearlyDegenerate_DoesNotCrash()
        {
            Assert.DoesNotThrow(() => _calculator.Calculate("2.0", "3.0", "4.999999"));
            var result = _calculator.Calculate("2.0", "3.0", "4.999999");
            foreach (var p in result.Coordinates)
            {
                Assert.That(p.X, Is.GreaterThanOrEqualTo(0));
                Assert.That(p.Y, Is.GreaterThanOrEqualTo(0));
            }
            Assert.That(result.Coordinates.Any(c => c.X == int.MinValue || c.Y == int.MinValue), Is.False,
                "Координаты не должны быть результатом приведения NaN");
        }

        [Test]
        public void Calculate_CoordinatesMatchSideLengthsApproximately()
        {
            var result = _calculator.Calculate("6", "7", "8");
            var pts = result.Coordinates;
            double Dist((int X, int Y) p, (int X, int Y) q) =>
                Math.Sqrt(Math.Pow(p.X - q.X, 2) + Math.Pow(p.Y - q.Y, 2));

            double sideA = Dist(pts[2], pts[1]);
            double sideB = Dist(pts[0], pts[2]);
            double sideC = Dist(pts[0], pts[1]);

            double sum = sideA + sideB + sideC;
            Assert.That(sideA / sum, Is.EqualTo(6.0 / (6 + 7 + 8)).Within(0.1));
            Assert.That(sideB / sum, Is.EqualTo(7.0 / (6 + 7 + 8)).Within(0.1));
            Assert.That(sideC / sum, Is.EqualTo(8.0 / (6 + 7 + 8)).Within(0.1));
        }
    }
}