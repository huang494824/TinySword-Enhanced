using System.Globalization;
using NUnit.Framework;

public class SaveJsonCodecTests
{
    private const string ValidJson = "{\"version\":1,\"positionX\":1.25,\"positionY\":-2.5,\"positionZ\":0,\"coinNum\":10}";

    [TestCase(0, 0f, 0f, 0f)]
    [TestCase(10, 1.25f, -2.5f, 0f)]
    [TestCase(int.MaxValue, -100f, 200f, -3.75f)]
    public void ValidV1_RoundTrips(int coins, float x, float y, float z)
    {
        var original = new SaveData { version = 1, coinNum = coins, positionX = x, positionY = y, positionZ = z };
        Assert.That(SaveJsonCodec.TrySerialize(original, out string json, out string error), Is.True, error);
        Assert.That(SaveJsonCodec.TryDeserialize(json, out SaveData restored, out error), Is.True, error);
        Assert.That(restored, Is.Not.SameAs(original));
        Assert.That(restored.version, Is.EqualTo(1));
        Assert.That(restored.coinNum, Is.EqualTo(coins));
        Assert.That(restored.positionX, Is.EqualTo(x));
        Assert.That(restored.positionY, Is.EqualTo(y));
        Assert.That(restored.positionZ, Is.EqualTo(z));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" \r\n\t ")]
    [TestCase("{}")]
    [TestCase("null")]
    [TestCase("not json")]
    [TestCase("{\"version\":")]
    [TestCase("{\"version\":1, broken}")]
    public void EmptyOrMalformedJson_FailsWithoutData(string json)
    {
        AssertRejected(json);
    }

    [TestCase("\"version\":1,")]
    [TestCase("\"positionX\":1.25,")]
    [TestCase("\"positionY\":-2.5,")]
    [TestCase("\"positionZ\":0,")]
    [TestCase(",\"coinNum\":10")]
    public void MissingRequiredField_FailsWithoutData(string field)
    {
        AssertRejected(ValidJson.Replace(field, ""));
    }

    [TestCase("\"version\":1", "\"version\":0")]
    [TestCase("\"version\":1", "\"version\":2")]
    [TestCase("\"version\":1", "\"version\":-1")]
    [TestCase("\"coinNum\":10", "\"coinNum\":-1")]
    [TestCase("\"version\":1", "\"version\":1.5")]
    [TestCase("\"coinNum\":10", "\"coinNum\":10.5")]
    [TestCase("\"version\":1", "\"version\":2147483648")]
    [TestCase("\"coinNum\":10", "\"coinNum\":2147483648")]
    [TestCase("\"version\":1", "\"version\":\"1\"")]
    [TestCase("\"version\":1", "\"version\":null")]
    [TestCase("\"coinNum\":10", "\"coinNum\":null")]
    [TestCase("\"positionX\":1.25", "\"positionX\":\"1.25\"")]
    [TestCase("\"version\":1", "\"version\":{}")]
    [TestCase("\"coinNum\":10", "\"coinNum\":[]")]
    [TestCase("\"positionX\":1.25", "\"positionX\":{}")]
    [TestCase("\"positionY\":-2.5", "\"positionY\":[]")]
    [TestCase("\"positionZ\":0", "\"positionZ\":\"invalid\"")]
    public void InvalidValuesOrFieldTypes_FailWithoutData(string field, string replacement)
    {
        AssertRejected(ValidJson.Replace(field, replacement));
    }

    [Test]
    public void NullPositionField_FailsWithoutData()
    {
        AssertRejected(ValidJson.Replace("\"positionX\":1.25", "\"positionX\":null"));
    }

    [Test]
    public void NumericStringField_FailsWithoutData()
    {
        AssertRejected(ValidJson.Replace("\"coinNum\":10", "\"coinNum\":\"10\""));
    }

    [Test]
    public void ValidJsonWithTrailingGarbage_FailsWithoutData()
    {
        AssertRejected(ValidJson + " trailing garbage");
    }

    [Test]
    public void NullData_CannotValidateOrSerialize()
    {
        Assert.That(SaveJsonCodec.TryValidate(null, out string error), Is.False);
        Assert.That(error, Is.Not.Null.And.Not.Empty);
        Assert.That(SaveJsonCodec.TrySerialize(null, out string json, out error), Is.False);
        Assert.That(json, Is.Null);
        Assert.That(error, Is.Not.Null.And.Not.Empty);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void EachNonFiniteAxis_CannotValidateOrSerialize(int axis)
    {
        foreach (float value in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
        {
            var data = new SaveData { version = 1, coinNum = 0 };
            if (axis == 0) data.positionX = value;
            if (axis == 1) data.positionY = value;
            if (axis == 2) data.positionZ = value;
            Assert.That(SaveJsonCodec.TryValidate(data, out string error), Is.False);
            Assert.That(error, Is.Not.Null.And.Not.Empty);
            Assert.That(SaveJsonCodec.TrySerialize(data, out string json, out error), Is.False);
            Assert.That(json, Is.Null);
        }
    }

    [TestCase("NaN")]
    [TestCase("Infinity")]
    [TestCase("-Infinity")]
    [TestCase("1e100")]
    public void NonFiniteJsonCoordinates_FailWithoutData(string value)
    {
        AssertRejected(ValidJson.Replace("1.25", value));
        AssertRejected(ValidJson.Replace("-2.5", value));
        AssertRejected(ValidJson.Replace("\"positionZ\":0", "\"positionZ\":" + value));
    }

    [Test]
    public void RepeatedDeserialize_DoesNotShareOrLeakCandidateValues()
    {
        Assert.That(SaveJsonCodec.TryDeserialize(ValidJson, out SaveData first, out string error), Is.True, error);
        Assert.That(SaveJsonCodec.TryDeserialize(ValidJson, out SaveData second, out error), Is.True, error);
        Assert.That(second, Is.Not.SameAs(first));
        first.coinNum = 999;
        first.positionX = 999;
        Assert.That(second.coinNum, Is.EqualTo(10));
        Assert.That(second.positionX, Is.EqualTo(1.25f));
        AssertRejected(ValidJson.Replace(",\"coinNum\":10", ""));
        AssertRejected(ValidJson.Replace("\"positionX\":1.25,", ""));
        AssertRejected("{}");
    }

    [TestCase("en-US", "fr-FR")]
    [TestCase("fr-FR", "de-DE")]
    [TestCase("de-DE", "en-US")]
    public void Json_RoundTripsAcrossCultures(string writeCulture, string readCulture)
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(writeCulture);
            var data = new SaveData { version = 1, positionX = 1.25f, positionY = -2.5f, coinNum = 10 };
            Assert.That(SaveJsonCodec.TrySerialize(data, out string json, out string error), Is.True, error);
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(readCulture);
            Assert.That(SaveJsonCodec.TryDeserialize(json, out SaveData restored, out error), Is.True, error);
            Assert.That(restored.positionX, Is.EqualTo(1.25f));
            Assert.That(restored.positionY, Is.EqualTo(-2.5f));
            Assert.That(restored.positionZ, Is.Zero);
            Assert.That(restored.coinNum, Is.EqualTo(10));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    private static void AssertRejected(string json)
    {
        Assert.That(SaveJsonCodec.TryDeserialize(json, out SaveData data, out string error), Is.False, json);
        Assert.That(data, Is.Null);
        Assert.That(error, Is.Not.Null.And.Not.Empty);
    }
}
