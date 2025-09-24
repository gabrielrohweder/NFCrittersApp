import { useState } from "react";
import { motion } from "framer-motion";
import { Button } from "@/components/ui/button";
import { Waves, Zap } from "lucide-react";

interface NFCScanButtonProps {
  onScan?: (animalId: string) => void;
  isScanning?: boolean;
}

export default function NFCScanButton({ onScan, isScanning = false }: NFCScanButtonProps) {
  const [scanAnimation, setScanAnimation] = useState(false);

  const handleScan = () => {
    setScanAnimation(true);
    console.log('NFC scan initiated');
    
    // Simulate NFC scan delay
    setTimeout(() => {
      const mockAnimalIds = ['lion-001', 'elephant-002', 'penguin-003', 'panda-004', 'owl-005', 'dolphin-006'];
      const randomId = mockAnimalIds[Math.floor(Math.random() * mockAnimalIds.length)];
      onScan?.(randomId);
      setScanAnimation(false);
    }, 2000);
  };

  return (
    <div className="relative flex flex-col items-center gap-4">
      {(isScanning || scanAnimation) && (
        <motion.div
          className="absolute inset-0 rounded-full border-2 border-primary"
          animate={{ scale: [1, 1.2, 1.4], opacity: [1, 0.7, 0] }}
          transition={{ duration: 1.5, repeat: Infinity }}
        />
      )}
      
      <motion.div
        whileHover={{ scale: 1.05 }}
        whileTap={{ scale: 0.95 }}
      >
        <Button
          size="lg"
          className="w-32 h-32 rounded-full bg-gradient-to-br from-primary to-accent hover:from-primary/90 hover:to-accent/90 text-primary-foreground shadow-lg"
          onClick={handleScan}
          disabled={isScanning || scanAnimation}
          data-testid="button-nfc-scan"
        >
          <motion.div
            animate={isScanning || scanAnimation ? { rotate: 360 } : {}}
            transition={{ duration: 2, repeat: Infinity, ease: "linear" }}
            className="flex flex-col items-center gap-2"
          >
            {isScanning || scanAnimation ? (
              <Waves className="w-8 h-8" />
            ) : (
              <Zap className="w-8 h-8" />
            )}
            <span className="text-sm font-semibold">
              {isScanning || scanAnimation ? 'Scanning...' : 'Scan NFC'}
            </span>
          </motion.div>
        </Button>
      </motion.div>
      
      <p className="text-sm text-muted-foreground text-center max-w-48">
        {isScanning || scanAnimation 
          ? 'Hold your phone near the NFC tag...' 
          : 'Tap to scan an NFC tag and discover new animals!'
        }
      </p>
    </div>
  );
}