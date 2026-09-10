using System;
using System.IO;
using NUnit.Framework;

public class SaveFileStoreTests
{
    private string directory;
    private string path;

    [SetUp]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "TinySword.SaveSystem.Tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        path = Path.Combine(directory, "save.json");
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }

    [Test]
    public void MissingFile_FailsWithoutData()
    {
        Assert.That(SaveFileStore.TryRead(path, out SaveData data, out string error), Is.False);
        Assert.That(data, Is.Null);
        Assert.That(error, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void ValidWriteRead_AndOverwrite()
    {
        var snapshot = new SaveData { version = 1, coinNum = 10, positionX = -1.25f, positionY = 2.5f, positionZ = 3f };
        Assert.That(SaveFileStore.TryWrite(path, snapshot, out string error), Is.True, error);
        Assert.That(SaveFileStore.TryRead(path, out SaveData first, out error), Is.True, error);
        Assert.That(first.version, Is.EqualTo(1));
        Assert.That(first.coinNum, Is.EqualTo(10));
        Assert.That(first.positionX, Is.EqualTo(-1.25f));
        Assert.That(first.positionY, Is.EqualTo(2.5f));
        Assert.That(first.positionZ, Is.EqualTo(3f));

        snapshot.coinNum = 0;
        snapshot.positionX = 0;
        Assert.That(SaveFileStore.TryWrite(path, snapshot, out error), Is.True, error);
        Assert.That(SaveFileStore.TryRead(path, out SaveData second, out error), Is.True, error);
        Assert.That(second.coinNum, Is.Zero);
        Assert.That(second.positionX, Is.Zero);
        Assert.That(first.coinNum, Is.EqualTo(10));
    }

    [Test]
    public void InvalidSnapshot_DoesNotOverwriteExistingFile()
    {
        var snapshot = new SaveData { version = 1, coinNum = 10 };
        Assert.That(SaveFileStore.TryWrite(path, snapshot, out string error), Is.True, error);
        string original = File.ReadAllText(path);
        snapshot.coinNum = -1;
        Assert.That(SaveFileStore.TryWrite(path, snapshot, out error), Is.False);
        Assert.That(error, Is.Not.Null.And.Not.Empty);
        Assert.That(File.ReadAllText(path), Is.EqualTo(original));
    }

    [Test]
    public void MissingParentDirectory_WriteReturnsFailure()
    {
        string missingParentPath = Path.Combine(directory, "missing", "save.json");
        Assert.That(SaveFileStore.TryWrite(missingParentPath, new SaveData { version = 1 }, out string error), Is.False);
        Assert.That(error, Is.Not.Null.And.Not.Empty);
        Assert.That(File.Exists(missingParentPath), Is.False);
    }

    [TestCase(null)]
    [TestCase("")]
    public void InvalidPath_ReturnsFailure(string invalidPath)
    {
        Assert.That(SaveFileStore.TryRead(invalidPath, out SaveData data, out string readError), Is.False);
        Assert.That(data, Is.Null);
        Assert.That(readError, Is.Not.Null.And.Not.Empty);
        Assert.That(SaveFileStore.TryWrite(invalidPath, new SaveData { version = 1 }, out string writeError), Is.False);
        Assert.That(writeError, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void CorruptedFile_FailsWithoutData()
    {
        File.WriteAllText(path, "{}");
        Assert.That(SaveFileStore.TryRead(path, out SaveData data, out string error), Is.False);
        Assert.That(data, Is.Null);
        Assert.That(error, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void LegacyFile_IsNotReadOrModified()
    {
        string legacyPath = Path.Combine(directory, "save.txt");
        File.WriteAllText(legacyPath, "1,2,3,10");
        Assert.That(SaveFileStore.TryRead(path, out SaveData data, out string error), Is.False);
        Assert.That(data, Is.Null);
        Assert.That(error, Is.Not.Null.And.Not.Empty);
        Assert.That(File.ReadAllText(legacyPath), Is.EqualTo("1,2,3,10"));
    }
}
