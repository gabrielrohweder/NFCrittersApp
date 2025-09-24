import { useState } from "react";
import { motion } from "framer-motion";
import NFCScanButton from "@/components/NFCScanButton";
import AnimalModal from "@/components/AnimalModal";
import AuthModal from "@/components/AuthModal";
import type { Animal } from "@/components/AnimalCard";
import lionImage from '@assets/generated_images/3D_cartoon_lion_7678e3f1.png';
import elephantImage from '@assets/generated_images/3D_cartoon_elephant_28b0801d.png';
import penguinImage from '@assets/generated_images/3D_cartoon_penguin_99368114.png';
import pandaImage from '@assets/generated_images/3D_cartoon_red_panda_5e73b17f.png';
import owlImage from '@assets/generated_images/3D_cartoon_owl_ea0b0b2d.png';
import dolphinImage from '@assets/generated_images/3D_cartoon_dolphin_5c46262b.png';

// todo: remove mock functionality - replace with real animal data from backend
const mockAnimals: Record<string, Animal> = {
  'lion-001': {
    id: 'lion-001',
    name: 'Savanna Lion',
    species: 'Panthera leo',
    habitat: 'African Grasslands',
    rarity: 'rare',
    imageUrl: lionImage,
    facts: [
      'Lions are the only cats that live in groups called prides',
      'A lion\'s roar can be heard up to 5 miles away',
      'Lions sleep 16-20 hours per day',
      'Male lions can weigh up to 420 pounds'
    ],
    collected: true
  },
  'elephant-002': {
    id: 'elephant-002',
    name: 'African Elephant',
    species: 'Loxodonta africana',
    habitat: 'African Savannas and Forests',
    rarity: 'legendary',
    imageUrl: elephantImage,
    facts: [
      'Elephants can weigh up to 13,000 pounds',
      'They have excellent memories and can remember other elephants for decades',
      'Elephants use their trunks like we use our hands',
      'Baby elephants are called calves and stay close to their mothers'
    ],
    collected: true
  },
  'penguin-003': {
    id: 'penguin-003',
    name: 'Emperor Penguin',
    species: 'Aptenodytes forsteri',
    habitat: 'Antarctic Ice Sheets',
    rarity: 'common',
    imageUrl: penguinImage,
    facts: [
      'Emperor penguins can dive up to 500 meters deep',
      'Males incubate eggs on their feet for 64 days',
      'They can hold their breath for up to 22 minutes',
      'Penguins huddle together to stay warm in temperatures below -40°C'
    ],
    collected: true
  },
  'panda-004': {
    id: 'panda-004',
    name: 'Red Panda',
    species: 'Ailurus fulgens',
    habitat: 'Himalayan Mountain Forests',
    rarity: 'rare',
    imageUrl: pandaImage,
    facts: [
      'Red pandas are also called firefox',
      'They are excellent climbers and spend most of their time in trees',
      'Red pandas have a false thumb to help them grasp bamboo',
      'They use their bushy tails as blankets in cold weather'
    ],
    collected: true
  },
  'owl-005': {
    id: 'owl-005',
    name: 'Great Horned Owl',
    species: 'Bubo virginianus',
    habitat: 'North American Forests',
    rarity: 'common',
    imageUrl: owlImage,
    facts: [
      'Great horned owls can rotate their heads 270 degrees',
      'They have excellent night vision and hearing',
      'Their feathers allow them to fly completely silently',
      'They are fierce hunters and can catch prey much larger than themselves'
    ],
    collected: true
  },
  'dolphin-006': {
    id: 'dolphin-006',
    name: 'Bottlenose Dolphin',
    species: 'Tursiops truncatus',
    habitat: 'Ocean Waters Worldwide',
    rarity: 'rare',
    imageUrl: dolphinImage,
    facts: [
      'Dolphins are highly intelligent and can recognize themselves in mirrors',
      'They communicate using clicks, whistles, and body language',
      'Dolphins can jump up to 20 feet out of the water',
      'They live in groups called pods and work together to hunt'
    ],
    collected: true
  }
};

interface ScanPageProps {
  isLoggedIn?: boolean;
  onLogin?: () => void;
}

export default function ScanPage({ isLoggedIn = false, onLogin }: ScanPageProps) {
  const [scannedAnimal, setScannedAnimal] = useState<Animal | null>(null);
  const [showAnimalModal, setShowAnimalModal] = useState(false);
  const [showAuthModal, setShowAuthModal] = useState(false);
  const [isScanning, setIsScanning] = useState(false);

  const handleScan = (animalId: string) => {
    console.log('NFC scan detected:', animalId);
    setIsScanning(true);
    
    // Simulate NFC processing time
    setTimeout(() => {
      const animal = mockAnimals[animalId];
      if (animal) {
        setScannedAnimal(animal);
        setShowAnimalModal(true);
      }
      setIsScanning(false);
    }, 1500);
  };

  const handleAddToCollection = (animalId: string) => {
    console.log('Adding to collection:', animalId);
    setShowAnimalModal(false);
    // todo: replace with real API call to add animal to user collection
  };

  const handleAuthRequired = () => {
    setShowAnimalModal(false);
    setShowAuthModal(true);
  };

  const handleAuthSuccess = () => {
    setShowAuthModal(false);
    onLogin?.();
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-muted/20 to-accent/10 flex flex-col">
      <div className="flex-1 flex flex-col items-center justify-center p-6 space-y-8">
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
          className="text-center space-y-4"
        >
          <h1 className="font-playful text-4xl text-foreground" data-testid="text-app-title">
            Animal Collector
          </h1>
          <p className="text-muted-foreground max-w-xs">
            Discover amazing 3D printed animals by scanning NFC tags!
          </p>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, scale: 0.8 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.6, delay: 0.2 }}
        >
          <NFCScanButton 
            onScan={handleScan}
            isScanning={isScanning}
          />
        </motion.div>

        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.6, delay: 0.4 }}
          className="text-center space-y-2"
        >
          <p className="text-xs text-muted-foreground">
            Hold your phone near any NFC-enabled animal figure
          </p>
          {!isLoggedIn && (
            <p className="text-xs text-accent">
              Login to save animals to your collection!
            </p>
          )}
        </motion.div>
      </div>

      <AnimalModal
        animal={scannedAnimal}
        isOpen={showAnimalModal}
        onClose={() => setShowAnimalModal(false)}
        onAddToCollection={handleAddToCollection}
        onAuthRequired={handleAuthRequired}
        isLoggedIn={isLoggedIn}
      />

      <AuthModal
        isOpen={showAuthModal}
        onClose={() => setShowAuthModal(false)}
        onLogin={() => handleAuthSuccess()}
        onSignup={() => handleAuthSuccess()}
      />
    </div>
  );
}