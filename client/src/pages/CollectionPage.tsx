import { useState } from "react";
import { motion } from "framer-motion";
import CollectionGrid from "@/components/CollectionGrid";
import AnimalModal from "@/components/AnimalModal";
import AuthModal from "@/components/AuthModal";
import { Button } from "@/components/ui/button";
import { User } from "lucide-react";
import type { Animal } from "@/components/AnimalCard";
import lionImage from '@assets/generated_images/3D_cartoon_lion_7678e3f1.png';
import elephantImage from '@assets/generated_images/3D_cartoon_elephant_28b0801d.png';
import penguinImage from '@assets/generated_images/3D_cartoon_penguin_99368114.png';
import pandaImage from '@assets/generated_images/3D_cartoon_red_panda_5e73b17f.png';
import owlImage from '@assets/generated_images/3D_cartoon_owl_ea0b0b2d.png';
import dolphinImage from '@assets/generated_images/3D_cartoon_dolphin_5c46262b.png';

// todo: remove mock functionality - replace with real user collection data from backend
const mockCollectionAnimals: Animal[] = [
  {
    id: 'lion-001',
    name: 'Savanna Lion',
    species: 'Panthera leo',
    habitat: 'African Grasslands',
    rarity: 'rare',
    imageUrl: lionImage,
    facts: [
      'Lions are the only cats that live in groups called prides',
      'A lion\'s roar can be heard up to 5 miles away',
      'Lions sleep 16-20 hours per day'
    ],
    collected: true
  },
  {
    id: 'elephant-002',
    name: 'African Elephant',
    species: 'Loxodonta africana',
    habitat: 'African Savannas and Forests',
    rarity: 'legendary',
    imageUrl: elephantImage,
    facts: [
      'Elephants can weigh up to 13,000 pounds',
      'They have excellent memories',
      'Elephants use their trunks like hands'
    ],
    collected: true
  },
  {
    id: 'penguin-003',
    name: 'Emperor Penguin',
    species: 'Aptenodytes forsteri',
    habitat: 'Antarctic Ice Sheets',
    rarity: 'common',
    imageUrl: penguinImage,
    facts: [
      'Can dive up to 500 meters deep',
      'Males incubate eggs for 64 days',
      'Can hold breath for 22 minutes'
    ],
    collected: false
  },
  {
    id: 'panda-004',
    name: 'Red Panda',
    species: 'Ailurus fulgens',
    habitat: 'Himalayan Mountain Forests',
    rarity: 'rare',
    imageUrl: pandaImage,
    facts: [
      'Also called firefox',
      'Excellent climbers',
      'Use tails as blankets'
    ],
    collected: false
  },
  {
    id: 'owl-005',
    name: 'Great Horned Owl',
    species: 'Bubo virginianus',
    habitat: 'North American Forests',
    rarity: 'common',
    imageUrl: owlImage,
    facts: [
      'Can rotate heads 270 degrees',
      'Excellent night vision',
      'Fly completely silently'
    ],
    collected: true
  },
  {
    id: 'dolphin-006',
    name: 'Bottlenose Dolphin',
    species: 'Tursiops truncatus',
    habitat: 'Ocean Waters Worldwide',
    rarity: 'rare',
    imageUrl: dolphinImage,
    facts: [
      'Highly intelligent',
      'Can jump 20 feet high',
      'Live in groups called pods'
    ],
    collected: false
  }
];

interface CollectionPageProps {
  isLoggedIn?: boolean;
  onLogin?: () => void;
}

export default function CollectionPage({ isLoggedIn = false, onLogin }: CollectionPageProps) {
  const [selectedAnimal, setSelectedAnimal] = useState<Animal | null>(null);
  const [showAnimalModal, setShowAnimalModal] = useState(false);
  const [showAuthModal, setShowAuthModal] = useState(false);

  const handleAnimalClick = (animal: Animal) => {
    if (animal.collected) {
      setSelectedAnimal(animal);
      setShowAnimalModal(true);
    }
  };

  const handleAuthSuccess = () => {
    setShowAuthModal(false);
    onLogin?.();
  };

  if (!isLoggedIn) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-background via-muted/20 to-accent/10 flex flex-col items-center justify-center p-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="text-center space-y-6"
        >
          <div className="text-8xl">🔒</div>
          <div className="space-y-2">
            <h2 className="font-playful text-2xl text-foreground">
              Collection Locked
            </h2>
            <p className="text-muted-foreground max-w-xs">
              Create an account to start building your animal collection!
            </p>
          </div>
          
          <Button 
            onClick={() => setShowAuthModal(true)}
            className="bg-gradient-to-r from-primary to-accent hover:from-primary/90 hover:to-accent/90"
            data-testid="button-login-to-view"
          >
            <User className="w-4 h-4 mr-2" />
            Login to View Collection
          </Button>
        </motion.div>

        <AuthModal
          isOpen={showAuthModal}
          onClose={() => setShowAuthModal(false)}
          onLogin={handleAuthSuccess}
          onSignup={handleAuthSuccess}
        />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-muted/20 to-accent/10 pb-20">
      <div className="p-6 space-y-6">
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6 }}
          className="text-center"
        >
          <h1 className="font-playful text-3xl text-foreground mb-2">
            My Collection
          </h1>
          <p className="text-sm text-muted-foreground">
            Your discovered animals and progress
          </p>
        </motion.div>

        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.6, delay: 0.2 }}
        >
          <CollectionGrid
            animals={mockCollectionAnimals}
            onAnimalClick={handleAnimalClick}
            title="Discovered Animals"
          />
        </motion.div>
      </div>

      <AnimalModal
        animal={selectedAnimal}
        isOpen={showAnimalModal}
        onClose={() => setShowAnimalModal(false)}
        isLoggedIn={isLoggedIn}
      />
    </div>
  );
}