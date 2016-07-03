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
            boolean[] data = {false, true, true, false};

            class DeeplyNested {
                char kek = 'k';
            }
        }
        Inner inner = new Inner();
        for (boolean item : inner.data) {
            System.out.println(item && true);
        }
        flag = c == inner.wow;
        System.out.println(flag);
    }
}
