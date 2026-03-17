public class PretrazivacNit extends Thread{
    private Integer pocektak,kraj,trazeni,brojNiti;
    public PretrazivacNit(Integer pocektak, Integer kraj, Integer trazeni, Integer brojNiti)
    {
        this.pocektak=pocektak;
        this.kraj=kraj;
        this.trazeni=trazeni;
        this.brojNiti=brojNiti;
    }

    public void run() {
        for (int i = pocektak; i < kraj && !Main.pronadjeno; i++) {
            if (Main.brojevi.get(i).equals(trazeni)) {
                Main.pronadjeno = true;
                System.out.println(
                        "Traženi broj " + trazeni +
                                " pronađen u niti " + brojNiti
                );
                break;
            }
        }
    }

}
