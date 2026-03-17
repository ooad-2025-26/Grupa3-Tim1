import java.util.ArrayList;
import java.util.List;
import java.util.Random;


public class Main {
    public static List<Integer> brojevi = new ArrayList<Integer>();
    public static boolean pronadjeno = false;
    public static void main(String[] args) {
        Random random = new Random();
        List<Thread> niti = new ArrayList<Thread>();
        for(int i=0;i<100000000;i++)
        {
            brojevi.add(random.nextInt(0, 100000000));
        }
        Integer trazeni = random.nextInt(0,100000000);
        for(int i=0;i<16;i++)
        {
            Integer pocetak = i * brojevi.size() / 16,
                    kraj = (i + 1) * brojevi.size() / 16;
            PretrazivacNit nit = new PretrazivacNit(pocetak, kraj,trazeni,i);
            niti.add(nit);
            nit.start();
        }
    }

}
