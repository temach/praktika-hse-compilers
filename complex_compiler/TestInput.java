class A {
    public static void Myfunc()
    {
        char k = 'K';
    }

    class GoodNested
    {
        boolean sunlight;
    }

    public static void main(String[] args) {
        boolean flag;
        char c = 'L';
        class Inner {
            char wow = 'A';
        }
        Inner inner = new Inner();
        flag = c == inner.wow;
        System.out.println(flag);
    }
}
