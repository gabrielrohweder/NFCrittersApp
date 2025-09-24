import { motion } from "framer-motion";
import AnimalCard from "./AnimalCard";
import type { Animal } from "./AnimalCard";

interface CollectionGridProps {
  animals: Animal[];
  onAnimalClick?: (animal: Animal) => void;
  title?: string;
}

export default function CollectionGrid({ animals, onAnimalClick, title = "My Collection" }: CollectionGridProps) {
  const collectedAnimals = animals.filter(animal => animal.collected);
  const totalAnimals = animals.length;
  const collectionPercentage = totalAnimals > 0 ? Math.round((collectedAnimals.length / totalAnimals) * 100) : 0;

  return (
    <div className="space-y-6">
      <div className="text-center space-y-2">
        <h2 className="font-playful text-2xl text-foreground" data-testid="text-collection-title">
          {title}
        </h2>
        <div className="space-y-2">
          <p className="text-sm text-muted-foreground" data-testid="text-collection-stats">
            {collectedAnimals.length} of {totalAnimals} collected ({collectionPercentage}%)
          </p>
          <div className="w-full bg-muted rounded-full h-2">
            <motion.div
              className="bg-gradient-to-r from-primary to-accent h-2 rounded-full"
              initial={{ width: 0 }}
              animate={{ width: `${collectionPercentage}%` }}
              transition={{ duration: 1, ease: "easeOut" }}
              data-testid="progress-collection"
            />
          </div>
        </div>
      </div>

      {animals.length === 0 ? (
        <div className="text-center py-12 space-y-4">
          <div className="text-6xl text-muted-foreground">🔍</div>
          <div>
            <h3 className="font-semibold text-lg">No animals yet!</h3>
            <p className="text-sm text-muted-foreground">
              Start scanning NFC tags to discover amazing animals
            </p>
          </div>
        </div>
      ) : (
        <motion.div 
          className="grid grid-cols-2 gap-4"
          initial="hidden"
          animate="visible"
          variants={{
            hidden: {},
            visible: {
              transition: {
                staggerChildren: 0.1
              }
            }
          }}
        >
          {animals.map((animal, index) => (
            <motion.div
              key={animal.id}
              variants={{
                hidden: { opacity: 0, y: 20 },
                visible: { opacity: 1, y: 0 }
              }}
              transition={{ duration: 0.3 }}
            >
              <AnimalCard
                animal={animal}
                onClick={() => onAnimalClick?.(animal)}
                isNew={false}
              />
            </motion.div>
          ))}
        </motion.div>
      )}
    </div>
  );
}