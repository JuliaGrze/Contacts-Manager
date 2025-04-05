namespace CRUDTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            //Arramge - declaration of variables and correct inputs
            MyMath mm = new MyMath();
            int input1 = 10, input2 = 5;
            int expected = 15;

            //Act - calling method
            int actual = mm.Add(input1, input2);

            //Assert - compare expected value with actual value
            Assert.Equal(expected, actual);
        }
    }
}
